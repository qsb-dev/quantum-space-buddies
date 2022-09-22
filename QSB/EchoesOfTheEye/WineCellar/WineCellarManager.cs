using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.WineCellar;

internal class WineCellarManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public async override UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		=> QSBWorldSync.Init<QSBWineCellarSwitch, WineCellarSwitch>();
}
