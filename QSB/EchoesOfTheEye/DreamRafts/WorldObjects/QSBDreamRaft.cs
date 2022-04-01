using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamRafts.WorldObjects;

public class QSBDreamRaft : LinkedWorldObject<DreamRaftController, RaftTransformSync>
{
	public override void SendInitialState(uint to) { }

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.RaftPrefab;
	protected override bool SpawnWithServerAuthority => false;
}
