using QuantumUNET;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Animation.Player.Thrusters
{
	public class JetpackAccelerationSync : QNetworkBehaviour
	{
		[SyncVar]
		private Vector3 _localAcceleration;
		[SyncVar]
		private bool _isThrusting;

		private ThrusterModel _thrusterModel;

		public Vector3 LocalAcceleration => _localAcceleration;
		public bool IsThrusting => _isThrusting;

		public void Init(ThrusterModel model) 
			=> _thrusterModel = model;

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
				_localAcceleration = _thrusterModel.GetLocalAcceleration();
				_isThrusting = _thrusterModel.IsTranslationalThrusterFiring();
			}
		}
	}
}
