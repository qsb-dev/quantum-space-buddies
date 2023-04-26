using Mirror;
using QSB.Player;
using QSB.Player.TransformSync;
using QSB.Utility.VariableSync;
using UnityEngine;

namespace QSB.ShipSync;

public class ShipThrusterVariableSyncer : NetworkBehaviour
{
	public Vector3VariableSyncer AccelerationSyncer;

	private ShipThrusterModel _thrusterModel;
	private ShipThrusterAudio _thrusterAudio;

	public void Init()
	{
		_thrusterModel = Locator.GetShipBody().GetComponent<ShipThrusterModel>();
		_thrusterAudio = Locator.GetShipBody().GetComponentInChildren<ShipThrusterAudio>();
	}

	public void Update()
	{
		// bug : this doesn't account for autopilot
		// fixes #590
		if (ShipManager.Instance.CurrentFlyer == uint.MaxValue)
		{
			if (_thrusterModel)
			{
				AccelerationSyncer.Value = Vector3.zero;
			}

			return;
		}

		if (PlayerTransformSync.LocalInstance && QSBPlayerManager.LocalPlayer.FlyingShip)
		{
			GetFromShip();
			return;
		}

		if (AccelerationSyncer.public_HasChanged())
		{
			if (AccelerationSyncer.Value == Vector3.zero)
			{
				foreach (var item in ShipThrusterManager.ShipFlameControllers)
				{
					item.OnStopTranslationalThrust();
				}

				_thrusterAudio.OnStopTranslationalThrust();

				ShipThrusterManager.ShipWashController.OnStopTranslationalThrust();
			}
			else
			{
				foreach (var item in ShipThrusterManager.ShipFlameControllers)
				{
					item.OnStartTranslationalThrust();
				}

				_thrusterAudio.OnStartTranslationalThrust();

				ShipThrusterManager.ShipWashController.OnStartTranslationalThrust();
			}
		}
	}

	private void GetFromShip()
	{
		if (_thrusterModel)
		{
			AccelerationSyncer.Value = _thrusterModel.GetLocalAcceleration();
		}
	}
}
