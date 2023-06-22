using QSB.ShipSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.ShipSync.WorldObjects;

internal class QSBShipDetachableLeg : LinkedWorldObject<ShipDetachableLeg, ShipLegTransformSync>
{
	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.ShipLegPrefab;
	protected override bool SpawnWithServerOwnership => true;

	public override void SendInitialState(uint to)
	{
		// todo SendInitialState?
	}
}
