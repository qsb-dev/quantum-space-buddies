using QSB.EchoesOfTheEye.EclipseDoors.VariableSync;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;

internal class QSBEclipseDoorController : LinkedWorldObject<EclipseDoorController, EclipseDoorVariableSyncer>
{
	public override void SendInitialState(uint to) { }

	public override string ReturnLabel()
		=> $"{base.ReturnLabel()}\r\n- SyncerValue:{NetworkBehaviour.Value}\r\n- HasAuth:{NetworkBehaviour.hasAuthority}";

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.DoorPrefab;
}
