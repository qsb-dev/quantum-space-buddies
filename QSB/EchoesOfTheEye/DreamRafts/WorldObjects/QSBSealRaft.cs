using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.RaftSync.TransformSync;
using QSB.Utility.LinkedWorldObject;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamRafts.WorldObjects;

public class QSBSealRaft : LinkedWorldObject<SealRaftController, RaftTransformSync>
{
	public override void SendInitialState(uint to) { }

	public override async UniTask Init(CancellationToken ct) =>
		EnableDisableDetector.Add(AttachedObject.gameObject, this);

	protected override GameObject NetworkObjectPrefab => QSBNetworkManager.singleton.RaftPrefab;
	protected override bool SpawnWithServerAuthority => false;
}
