using QSB.Utility.VariableSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.Animation.Player.Thrusters
{
	public class JetpackAccelerationSync : QNetworkBehaviour
	{
		public Vector3VariableSyncer AccelerationVariableSyncer;
		public BoolVariableSyncer ThrustingVariableSyncer;
		public Vector3 LocalAcceleration { get; private set; }
		public bool IsThrusting { get; private set; }

		private ThrusterModel _thrusterModel;

		public void Init(ThrusterModel model)
		{
			_thrusterModel = model;

			AccelerationVariableSyncer.Init(() => LocalAcceleration, val => LocalAcceleration = val);
			ThrustingVariableSyncer.Init(() => IsThrusting, val => IsThrusting = val);
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
				LocalAcceleration = _thrusterModel.GetLocalAcceleration();
				IsThrusting = _thrusterModel.IsTranslationalThrusterFiring();
			}
		}
	}
}
