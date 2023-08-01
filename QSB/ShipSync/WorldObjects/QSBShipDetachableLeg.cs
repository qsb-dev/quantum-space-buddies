using QSB.ShipSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.ShipSync.WorldObjects;

public class QSBShipDetachableLeg : LinkedWorldObject<ShipDetachableLeg, ShipLegTransformSync>
{
	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.ShipLegPrefab;
	protected override bool SpawnWithServerOwnership => true;
}
