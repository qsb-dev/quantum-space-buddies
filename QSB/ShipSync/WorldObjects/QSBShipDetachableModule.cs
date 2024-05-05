using QSB.ShipSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.ShipSync.WorldObjects;

public class QSBShipDetachableModule : LinkedWorldObject<ShipDetachableModule, ShipModuleTransformSync>
{
	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.ShipModulePrefab;
	protected override bool SpawnWithServerOwnership => true;
}
