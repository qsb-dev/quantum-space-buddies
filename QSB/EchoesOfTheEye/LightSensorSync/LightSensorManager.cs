using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.LightSensorSync
{
	internal class LightSensorManager : WorldObjectManager
	{
		// see AirlockManager question
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken cancellationToken) => QSBWorldSync.Init<QSBSingleLightSensor, SingleLightSensor>();
	}
}
