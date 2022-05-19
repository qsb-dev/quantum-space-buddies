using Mirror;
using QSB.Player;
using QSB.Utility.VariableSync;
using UnityEngine;

namespace QSB.ShipSync;

public class ShipThrusterVariableSyncer : NetworkBehaviour
{
	public Vector3VariableSyncer AccelerationSyncer;

	private ShipThrusterModel _thrusterModel;

	public void Init()
	{
		_thrusterModel = Locator.GetShipBody().GetComponent<ShipThrusterModel>();
	}

	public void Update()
	{
		if (QSBPlayerManager.LocalPlayer.FlyingShip)
		{
			GetFromShip();
			return;
		}

		if (AccelerationSyncer.HasChanged())
		{
			if (AccelerationSyncer.Value == Vector3.zero)
			{
				foreach (var item in ShipThrusterManager.ShipFlameControllers)
				{
					item.OnStopTranslationalThrust();
				}

				ShipThrusterManager.ShipWashController.OnStopTranslationalThrust();
			}
			else
			{
				foreach (var item in ShipThrusterManager.ShipFlameControllers)
				{
					item.OnStartTranslationalThrust();
				}

				ShipThrusterManager.ShipWashController.OnStartTranslationalThrust();
			}
		}
	}

	private void GetFromShip() => AccelerationSyncer.Value = _thrusterModel.GetLocalAcceleration();
}
 