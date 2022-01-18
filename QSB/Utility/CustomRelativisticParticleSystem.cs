using QSB.Player;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Utility
{
	internal class CustomRelativisticParticleSystem : MonoBehaviour
	{
		private ParticleSystem _particleSystem;
		private Transform _simulationSpace;
		private Quaternion _rotation;
		private ParticleSystem.MainModule _mainModule;
		private ParticleSystem.VelocityOverLifetimeModule _velocityOverLifetimeModule;
		private ModuleVector _velocityOverLifetimeVector;
		private ParticleSystem.LimitVelocityOverLifetimeModule _limitVelocityOverLifetimeModule;
		private ModuleVector _limitVelocityOverLifetimeVector;
		private ParticleSystem.ForceOverLifetimeModule _forceOverLifetimeModule;
		private ModuleVector _forceOverLifetimeVector;
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
			_velocityOverLifetimeVector = new ModuleVector(_velocityOverLifetimeModule.x, _velocityOverLifetimeModule.y, _velocityOverLifetimeModule.z);
			_limitVelocityOverLifetimeModule = _particleSystem.limitVelocityOverLifetime;
			_limitVelocityOverLifetimeVector = new ModuleVector(_limitVelocityOverLifetimeModule.limitX, _limitVelocityOverLifetimeModule.limitY, _limitVelocityOverLifetimeModule.limitZ);
			_forceOverLifetimeModule = _particleSystem.forceOverLifetime;
			_forceOverLifetimeVector = new ModuleVector(_forceOverLifetimeModule.x, _forceOverLifetimeModule.y, _forceOverLifetimeModule.z);
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

		private struct ModuleVector
		{
			public ParticleSystem.MinMaxCurve OrigX;
			public ParticleSystem.MinMaxCurve OrigY;
			public ParticleSystem.MinMaxCurve OrigZ;

			public ModuleVector(ParticleSystem.MinMaxCurve x, ParticleSystem.MinMaxCurve y, ParticleSystem.MinMaxCurve z)
			{
				OrigX = x;
				OrigY = y;
				OrigZ = z;
			}

			public void GetRotatedVector(Quaternion rotation, out ParticleSystem.MinMaxCurve x, out ParticleSystem.MinMaxCurve y, out ParticleSystem.MinMaxCurve z)
			{
				if (OrigX.mode == ParticleSystemCurveMode.Constant)
				{
					var vector = rotation * new Vector3(OrigX.constant, OrigY.constant, OrigZ.constant);
					x = new ParticleSystem.MinMaxCurve(vector.x);
					y = new ParticleSystem.MinMaxCurve(vector.y);
					z = new ParticleSystem.MinMaxCurve(vector.z);
				}
				else if (OrigX.mode == ParticleSystemCurveMode.TwoConstants)
				{
					var vector2 = rotation * new Vector3(OrigX.constantMin, OrigY.constantMin, OrigZ.constantMin);
					var vector3 = rotation * new Vector3(OrigX.constantMax, OrigY.constantMax, OrigZ.constantMax);
					x = new ParticleSystem.MinMaxCurve(vector2.x, vector3.x);
					y = new ParticleSystem.MinMaxCurve(vector2.y, vector3.y);
					z = new ParticleSystem.MinMaxCurve(vector2.z, vector3.z);
				}
				else
				{
					Debug.LogWarning("Cannot properly rotate Module Curves! Use Constants mode instead, dummy.");
					x = OrigX;
					y = OrigY;
					z = OrigZ;
				}
			}
		}
	}
}
