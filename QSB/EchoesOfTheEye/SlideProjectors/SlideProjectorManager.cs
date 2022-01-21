using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.SlideProjectors
{
	internal class SlideProjectorManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override void BuildWorldObjects(OWScene scene) => QSBWorldSync.Init<QSBSlideProjector, SlideProjector>();
	}
}
