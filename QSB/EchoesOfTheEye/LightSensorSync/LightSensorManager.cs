using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.Utility.Deterministic;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.EchoesOfTheEye.LightSensorSync;

public class LightSensorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;
	public override bool DlcOnly => true;

	public static bool IsPlayerLightSensor(LightSensor lightSensor) => lightSensor.name is "CameraDetector" or "REMOTE_CameraDetector";

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		// ignore player light sensors
		var list = QSBWorldSync.GetUnityObjects<SingleLightSensor>()
			.Where(x => !IsPlayerLightSensor(x))
			.SortDeterministic();
		QSBWorldSync.Init<QSBLightSensor, SingleLightSensor>(list);
	}
}
