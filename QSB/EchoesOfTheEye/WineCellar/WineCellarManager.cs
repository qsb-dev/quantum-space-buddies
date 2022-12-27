using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.WineCellar.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.WineCellar;

internal class WineCellarManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		=> QSBWorldSync.Init<QSBWineCellarSwitch, WineCellarSwitch>();
}
