using QSB.Player;
using QSB.Utility;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters;

[UsedInUnityProject]
public class RemoteThrusterWashController : MonoBehaviour
{
	[SerializeField]
	private float _raycastDistance = 10f;

	[SerializeField]
	private AnimationCurve _emissionDistanceScale;

	[SerializeField]
	private AnimationCurve _emissionThrusterScale;

	[SerializeField]
	private ParticleSystem _defaultParticleSystem;

	private ParticleSystem.MainModule _defaultMainModule;
	private ParticleSystem.EmissionModule _defaultEmissionModule;
	private float _baseDefaultEmissionRate;

	private PlayerInfo _attachedPlayer;

	private bool _initialised;

	public void Init(PlayerInfo player)
	{
		_attachedPlayer = player;

		if (_defaultParticleSystem == null)
		{
			DebugLog.ToConsole($"Error - DefaultParticleSystem is null!", OWML.Common.MessageType.Error);
			return;
		}

		_defaultMainModule = _defaultParticleSystem.main;
		_defaultEmissionModule = _defaultParticleSystem.emission;
		_baseDefaultEmissionRate = _defaultEmissionModule.rateOverTime.constant;

		_initialised = true;
	}

	private void Update()
	{
		if (!_initialised)
		{
			return;
		}

		RaycastHit hitInfo = default;
		var aboveSurface = false;
		var emissionThrusterScale = _emissionThrusterScale.Evaluate(_attachedPlayer.JetpackAcceleration.AccelerationVariableSyncer.Value.y);
		if (emissionThrusterScale > 0f)
		{
			aboveSurface = Physics.Raycast(transform.position, transform.forward, out hitInfo, _raycastDistance, OWLayerMask.physicalMask);
		}

		emissionThrusterScale = (!aboveSurface) ? 0f : (emissionThrusterScale * _emissionDistanceScale.Evaluate(hitInfo.distance));

		if (emissionThrusterScale > 0f && _attachedPlayer.Visible)
		{
			var position = hitInfo.point + (hitInfo.normal * 0.25f);
			var rotation = Quaternion.LookRotation(hitInfo.normal);
			if (!_defaultParticleSystem.isPlaying)
			{
				_defaultParticleSystem.Play();
			}

			_defaultEmissionModule.rateOverTimeMultiplier = _baseDefaultEmissionRate * emissionThrusterScale;
			_defaultParticleSystem.transform.SetPositionAndRotation(position, rotation);
			if (_defaultMainModule.customSimulationSpace != hitInfo.transform)
			{
				_defaultMainModule.customSimulationSpace = hitInfo.transform;
				_defaultParticleSystem.Clear();
			}
		}
		else
		{
			if (_defaultParticleSystem.isPlaying)
			{
				_defaultParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
			}
		}
	}
}