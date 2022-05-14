using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.DreamCandles.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.DreamCandles;

public class DreamCandleManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBDreamCandle, DreamCandle>();
}
