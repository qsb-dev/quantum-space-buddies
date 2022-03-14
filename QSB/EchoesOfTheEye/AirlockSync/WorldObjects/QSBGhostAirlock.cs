using QSB.EchoesOfTheEye.AirlockSync.VariableSync;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AirlockSync.WorldObjects;

internal class QSBGhostAirlock : LinkedWorldObject<GhostAirlock, AirlockVariableSyncer>
{
	public override void SendInitialState(uint to) { }

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.AirlockPrefab;
	protected override bool SpawnWithServerAuthority => true;
}
