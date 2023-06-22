using HarmonyLib;
using QSB.EchoesOfTheEye.EclipseDoors.VariableSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;

internal class QSBEclipseDoorController : QSBRotatingElements<EclipseDoorController, EclipseDoorVariableSyncer>
{
	protected override IEnumerable<SingleLightSensor> LightSensors => AttachedObject._lightSensors;

	public override string ReturnLabel()
		=> $"{base.ReturnLabel()}\r\n- SyncerValue:{NetworkBehaviour.Value?.Join()}\r\n- IsOwned:{NetworkBehaviour.isOwned}";

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.DoorPrefab;
}
