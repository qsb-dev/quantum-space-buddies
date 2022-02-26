using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.LightSensorSync;

internal class LightSensorManager : WorldObjectManager
{
	// see AirlockManager question
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) => QSBWorldSync.Init<QSBLightSensor, SingleLightSensor>();
}