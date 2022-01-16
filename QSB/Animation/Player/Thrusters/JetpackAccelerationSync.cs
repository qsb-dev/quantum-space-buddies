using QSB.Utility.VariableSync;
using QuantumUNET;

namespace QSB.Animation.Player.Thrusters
{
	public class JetpackAccelerationSync : QNetworkBehaviour
	{
		public Vector3VariableSyncer AccelerationVariableSyncer;
		public BoolVariableSyncer ThrustingVariableSyncer;

		private ThrusterModel _thrusterModel;

		public void Init(ThrusterModel model)
		{
			_thrusterModel = model;

			AccelerationVariableSyncer.Init();
			ThrustingVariableSyncer.Init();
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
				AccelerationVariableSyncer.Value = _thrusterModel.GetLocalAcceleration();
				ThrustingVariableSyncer.Value = _thrusterModel.IsTranslationalThrusterFiring();
			}
		}
	}
}
