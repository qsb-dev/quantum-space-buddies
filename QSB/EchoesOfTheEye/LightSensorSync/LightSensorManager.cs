using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.LightSensorSync
{
	internal class LightSensorManager : WorldObjectManager
	{
		// see AirlockManager question
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override void BuildWorldObjects(OWScene scene) => QSBWorldSync.Init<QSBSingleLightSensor, SingleLightSensor>();
	}
}
