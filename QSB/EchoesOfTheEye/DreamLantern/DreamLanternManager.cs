using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.DreamLantern;

public class DreamLanternManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBDreamLanternController, DreamLanternController>();
}
