using Mirror;
using QSB.Utility.VariableSync;

namespace QSB.Animation.Player.Thrusters;

public class ThrusterSync : NetworkBehaviour
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