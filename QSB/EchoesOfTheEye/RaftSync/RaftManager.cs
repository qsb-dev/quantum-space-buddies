using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.RaftSync;

public class RaftManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBRaft, RaftController>();
		QSBWorldSync.Init<QSBRaftDock, RaftDock>();
		QSBWorldSync.Init<QSBDamRaftLift, DamRaftLift>();
	}
}
