using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.Sarcophagus.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.Sarcophagus;

public class SarcophagusManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBSarcophagus, SarcophagusController>();
}
