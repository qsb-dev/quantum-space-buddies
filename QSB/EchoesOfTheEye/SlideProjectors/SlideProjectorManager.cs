using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.SlideProjectors
{
	internal class SlideProjectorManager : WorldObjectManager
	{
		public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) => QSBWorldSync.Init<QSBSlideProjector, SlideProjector>();
	}
}