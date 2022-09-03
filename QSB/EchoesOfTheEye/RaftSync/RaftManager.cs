using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.EchoesOfTheEye.RaftSync;

public class RaftManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		// NH sometimes makes the body (but not the raft) null. what
		QSBWorldSync.Init<QSBRaft, RaftController>(QSBWorldSync.GetUnityObjects<RaftController>()
			.Where(x => x.GetAttachedOWRigidbody())
			.SortDeterministic());
		QSBWorldSync.Init<QSBRaftDock, RaftDock>();
	}
}
