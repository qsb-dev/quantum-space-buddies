using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.LightSensorSync;

internal class LightSensorManager : WorldObjectManager
{
	/// <summary>
	/// light sensor apparently shows up in eye
	/// </summary>
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;
	/// <summary>
	/// light sensor patches like to run even with no dlc
	/// </summary>
	public override bool DlcOnly => false;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBLightSensor, SingleLightSensor>();
}
