using QSB.Utility.VariableSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters
{
	public class JetpackAccelerationSync : QNetworkBehaviour
	{
		public Vector3VariableSyncer AccelerationVariableSyncer;
		public BoolVariableSyncer ThrustingVariableSyncer;
		public Vector3 LocalAcceleration => AccelerationVariableSyncer.ValueToSync.Value;
		public bool IsThrusting => ThrustingVariableSyncer.ValueToSync.Value;

		private Vector3 _localAcceleration;
		private bool _isThrusting;
		private ThrusterModel _thrusterModel;

		public void Init(ThrusterModel model)
		{
			_thrusterModel = model;

			AccelerationVariableSyncer.Init(() => _localAcceleration, val => _localAcceleration = val);
			ThrustingVariableSyncer.Init(() => _isThrusting, val => _isThrusting = val);
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
				AccelerationVariableSyncer.ValueToSync.Value = _thrusterModel.GetLocalAcceleration();
				ThrustingVariableSyncer.ValueToSync.Value = _thrusterModel.IsTranslationalThrusterFiring();
			}
		}
	}
}
