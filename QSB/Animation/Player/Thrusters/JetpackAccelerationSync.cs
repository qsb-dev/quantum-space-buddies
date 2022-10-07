using Mirror;
using QSB.Utility;
using QSB.Utility.VariableSync;

namespace QSB.Animation.Player.Thrusters;

[UsedInUnityProject]
public class JetpackAccelerationSync : NetworkBehaviour
{
	public Vector3VariableSyncer AccelerationVariableSyncer;

	private ThrusterModel _thrusterModel;

	public void Init(ThrusterModel model) => _thrusterModel = model;

	public void Update()
	{
		if (isLocalPlayer)
		{
			SyncLocalAccel();
		}
	}

	private void SyncLocalAccel()
	{
		if (_thrusterModel != null)
		{
			AccelerationVariableSyncer.Value = _thrusterModel.GetLocalAcceleration();
		}
	}
}