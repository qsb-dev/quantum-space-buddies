using QSB.ShipSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.ShipSync.WorldObjects;

internal class QSBShipDetachableLeg : LinkedWorldObject<ShipDetachableLeg, ShipLegTransformSync>
{
	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.ShipLegPrefab;
	protected override bool SpawnWithServerAuthority => true;

	public override void SendInitialState(uint to)
	{
		// todo SendInitialState?
	}
}
