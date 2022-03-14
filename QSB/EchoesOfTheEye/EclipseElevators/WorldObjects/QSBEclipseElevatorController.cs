using QSB.EchoesOfTheEye.EclipseElevators.VariableSync;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseElevators.WorldObjects;

internal class QSBEclipseElevatorController : LinkedWorldObject<EclipseElevatorController, EclipseElevatorVariableSyncer>
{
	public override void SendInitialState(uint to) { }

	public override string ReturnLabel()
		=> $"{base.ReturnLabel()}\r\n- SyncerValue:{NetworkBehaviour.Value}\r\n- HasAuth:{NetworkBehaviour.hasAuthority}";

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.ElevatorPrefab;
}
