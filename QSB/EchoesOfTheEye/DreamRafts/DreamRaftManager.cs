using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.DreamRafts;

public class DreamRaftManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBDreamRaftController, DreamRaftController>();
		QSBWorldSync.Init<QSBSealRaftController, SealRaftController>();

		QSBWorldSync.Init<QSBDreamObjectProjector, DreamObjectProjector>();
	}
}
