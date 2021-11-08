using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects
{
	class QSBSingleLightSensor : WorldObject<SingleLightSensor>
	{
		public override void Init(SingleLightSensor sensor, int id)
		{
			ObjectId = id;
			AttachedObject = sensor;
		}
	}
}
