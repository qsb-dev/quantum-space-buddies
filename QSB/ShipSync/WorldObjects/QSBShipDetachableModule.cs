using QSB.ShipSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.ShipSync.WorldObjects;

public class QSBShipDetachableModule : LinkedWorldObject<ShipDetachableModule, ShipModuleTransformSync>
{
	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.ShipModulePrefab;
	protected override bool SpawnWithServerOwnership => true;

	public override void SendInitialState(uint to)
	{
		// todo SendInitialState?
	}
}
