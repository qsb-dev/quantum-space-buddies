using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers;

internal class CodeControllerManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBEclipseCodeController, EclipseCodeController4>();
	}
}
