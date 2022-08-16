using QSB.ShipSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.ShipSync.WorldObjects;

internal class QSBShipDetachableModule : LinkedWorldObject<ShipDetachableModule, ShipModuleTransformSync>
{
	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.ShipModulePrefab;
	protected override bool SpawnWithServerAuthority => true;

	public override void SendInitialState(uint to)
	{
		// todo SendInitialState?
	}
}
