using QSB.Player;
using QSB.Utility;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters;

[UsedInUnityProject]
public class RemoteThrusterParticlesBehaviour : MonoBehaviour
{
	[SerializeField]
	private Thruster _thruster;

	[SerializeField]
	private bool _underwaterParticles;

	private bool _initialized;
	private PlayerInfo _attachedPlayer;
	private RemotePlayerFluidDetector _fluidDetector;
	private ParticleSystem _thrustingParticles;
	private Vector3 _thrusterFilter;
	private bool _underwater;

	public void Init(PlayerInfo player)
	{
		_attachedPlayer = player;
		_fluidDetector = player.FluidDetector;
		_thrustingParticles = gameObject.GetComponent<ParticleSystem>();
		_thrustingParticles.GetComponent<CustomRelativisticParticleSystem>().Init(player);
		_thrusterFilter = OWUtilities.GetShipThrusterFilter(_thruster);
		_underwater = false;
		_thrustingParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

		_fluidDetector.OnEnterFluidType += OnEnterExitFluidType;
		_fluidDetector.OnExitFluidType += OnEnterExitFluidType;

		_initialized = true;
	}

	private void OnDestroy()
	{
		_initialized = false;
		if (_fluidDetector != null)
		{
			_fluidDetector.OnEnterFluidType -= OnEnterExitFluidType;
			_fluidDetector.OnExitFluidType -= OnEnterExitFluidType;
		}
	}

	private void Update()
	{
		if (!_initialized)
		{
			return;
		}

		if (((_underwater != _underwaterParticles)
			? 0f
			: Vector3.Dot(_attachedPlayer.JetpackAcceleration.AccelerationVariableSyncer.Value, _thrusterFilter)) > 1f)
		{
			if (!_thrustingParticles.isPlaying)
			{
				_thrustingParticles.Play();
				return;
			}
		}
		else if (_thrustingParticles.isPlaying)
		{
			_thrustingParticles.Stop();
		}
	}

	private void OnEnterExitFluidType(FluidVolume.Type type)
	{
		_underwater = _fluidDetector.InFluidType(FluidVolume.Type.WATER);
	}
}
