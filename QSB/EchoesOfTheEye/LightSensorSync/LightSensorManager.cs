using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.LightSensorSync
{
	internal class LightSensorManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		protected override void RebuildWorldObjects(OWScene scene) => QSBWorldSync.Init<QSBSingleLightSensor, SingleLightSensor>();
	}
}
