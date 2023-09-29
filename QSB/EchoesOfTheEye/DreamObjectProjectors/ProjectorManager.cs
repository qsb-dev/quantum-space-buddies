using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.DreamObjectProjectors;

public class ProjectorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		=> QSBWorldSync.Init<QSBDreamObjectProjector, DreamObjectProjector>();
}
