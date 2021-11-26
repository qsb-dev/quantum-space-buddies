using QSB.Utility.VariableSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters
{
	public class JetpackAccelerationSync : QNetworkBehaviour
	{
		public Vector3VariableSyncer AccelerationVariableSyncer;
		public BoolVariableSyncer ThrustingVariableSyncer;
		public Vector3 LocalAcceleration => _accelerationValueReference.Value;
		public bool IsThrusting => _thrustingValueReference.Value;

		private VariableReference<Vector3> _accelerationValueReference;
		private VariableReference<bool> _thrustingValueReference;
		private Vector3 _localAcceleration;
		private bool _isThrusting;
		private ThrusterModel _thrusterModel;

		public void Init(ThrusterModel model)
		{
			_thrusterModel = model;

			_accelerationValueReference = new VariableReference<Vector3>(() => _localAcceleration, val => _localAcceleration = val);
			AccelerationVariableSyncer.FloatToSync = _accelerationValueReference;

			_thrustingValueReference = new VariableReference<bool>(() => _isThrusting, val => _isThrusting = val);
			ThrustingVariableSyncer.FloatToSync = _thrustingValueReference;
		}

		public void Update()
		{
			if (IsLocalPlayer)
			{
				SyncLocalAccel();
			}
		}

		private void SyncLocalAccel()
		{
			if (_thrusterModel != null)
			{
				_accelerationValueReference.Value = _thrusterModel.GetLocalAcceleration();
				_thrustingValueReference.Value = _thrusterModel.IsTranslationalThrusterFiring();
			}
		}
	}
}
