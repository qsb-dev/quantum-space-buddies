using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.EchoesOfTheEye.LightSensorSync;

internal class LightSensorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public static bool ShouldIgnore(LightSensor lightSensor) =>
		lightSensor.name is "CameraDetector" or "REMOTE_CameraDetector";

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		// ignore player light sensors
		var list = QSBWorldSync.GetUnityObjects<SingleLightSensor>()
			.Where(x => !ShouldIgnore(x))
			.SortDeterministic();
		QSBWorldSync.Init<QSBLightSensor, SingleLightSensor>(list);
	}
}
