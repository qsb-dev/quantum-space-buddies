using QSB.Player;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Utility;

[UsedInUnityProject]
internal class CustomRelativisticParticleSystem : MonoBehaviour
{
	private ParticleSystem _particleSystem;
	private Transform _simulationSpace;
	private Quaternion _rotation;
	private ParticleSystem.MainModule _mainModule;
	private ParticleSystem.VelocityOverLifetimeModule _velocityOverLifetimeModule;
	private RelativisticParticleSystem.ModuleVector _velocityOverLifetimeVector;
	private ParticleSystem.LimitVelocityOverLifetimeModule _limitVelocityOverLifetimeModule;
	private RelativisticParticleSystem.ModuleVector _limitVelocityOverLifetimeVector;
	private ParticleSystem.ForceOverLifetimeModule _forceOverLifetimeModule;
	private RelativisticParticleSystem.ModuleVector _forceOverLifetimeVector;
	private bool _isReady;

	private void Awake()
	{
		if (!_isReady)
		{
			DebugLog.ToConsole($"Warning - Awake() ran when _isReady is false!", OWML.Common.MessageType.Warning);
			return;
		}

		_particleSystem = GetComponent<ParticleSystem>();
		if (_particleSystem == null)
		{
			DebugLog.ToConsole($"Error - _particleSystem is null.", OWML.Common.MessageType.Error);
			_isReady = false;
			return;
		}

		_rotation = transform.rotation;
		_mainModule = _particleSystem.main;
		_mainModule.simulationSpace = ParticleSystemSimulationSpace.Custom;
		_mainModule.customSimulationSpace = _simulationSpace;
		_velocityOverLifetimeModule = _particleSystem.velocityOverLifetime;
		_velocityOverLifetimeVector = new RelativisticParticleSystem.ModuleVector(_velocityOverLifetimeModule.x, _velocityOverLifetimeModule.y, _velocityOverLifetimeModule.z);
		_limitVelocityOverLifetimeModule = _particleSystem.limitVelocityOverLifetime;
		_limitVelocityOverLifetimeVector = new RelativisticParticleSystem.ModuleVector(_limitVelocityOverLifetimeModule.limitX, _limitVelocityOverLifetimeModule.limitY, _limitVelocityOverLifetimeModule.limitZ);
		_forceOverLifetimeModule = _particleSystem.forceOverLifetime;
		_forceOverLifetimeVector = new RelativisticParticleSystem.ModuleVector(_forceOverLifetimeModule.x, _forceOverLifetimeModule.y, _forceOverLifetimeModule.z);
	}

	public void Init(PlayerInfo playerInfo)
	{
		var space = new GameObject($"{name}_ReferenceFrame");
		if (playerInfo.Body == null)
		{
			DebugLog.ToConsole($"Error - Player body is null! Did you create this too early?", OWML.Common.MessageType.Error);
			return;
		}

		space.transform.parent = playerInfo.Body.transform;
		_simulationSpace = space.transform;
		_isReady = true;
	}

	private void FixedUpdate()
	{
		if (!QSBWorldSync.AllObjectsReady || !_isReady)
		{
			return;
		}

		if (_simulationSpace == null)
		{
			DebugLog.ToConsole($"Error - _simulationSpace is null.", OWML.Common.MessageType.Error);
			_isReady = false;
		}

		_simulationSpace.rotation = _rotation;

		if (!_velocityOverLifetimeModule.enabled
		    && (!_limitVelocityOverLifetimeModule.enabled || !_limitVelocityOverLifetimeModule.separateAxes)
		    && !_forceOverLifetimeModule.enabled)
		{
			return;
		}

		var rotation = Quaternion.Inverse(_rotation) * transform.rotation;
		if (_velocityOverLifetimeModule.enabled)
		{
			_velocityOverLifetimeVector.GetRotatedVector(rotation, out var x, out var y, out var z);
			_velocityOverLifetimeModule.x = x;
			_velocityOverLifetimeModule.y = y;
			_velocityOverLifetimeModule.z = z;
		}

		if (_limitVelocityOverLifetimeModule.enabled)
		{
			_limitVelocityOverLifetimeVector.GetRotatedVector(rotation, out var x, out var y, out var z);
			_limitVelocityOverLifetimeModule.limitX = x;
			_limitVelocityOverLifetimeModule.limitY = y;
			_limitVelocityOverLifetimeModule.limitZ = z;
		}

		if (_forceOverLifetimeModule.enabled)
		{
			_forceOverLifetimeVector.GetRotatedVector(rotation, out var x, out var y, out var z);
			_forceOverLifetimeModule.x = x;
			_forceOverLifetimeModule.y = y;
			_forceOverLifetimeModule.z = z;
		}
	}
}