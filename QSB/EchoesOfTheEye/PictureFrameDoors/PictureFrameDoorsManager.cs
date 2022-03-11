using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.PictureFrameDoors;

internal class PictureFrameDoorsManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBPictureFrameDoorInterface, PictureFrameDoorInterface>(typeof(GlitchedCodeDoorInterface));
		QSBWorldSync.Init<QSBGlitchedCodeDoorInterface, GlitchedCodeDoorInterface>();
	}
}
