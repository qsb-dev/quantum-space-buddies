using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.AlarmTotemSync;

public class AlarmTotemManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBAlarmTotem, AlarmTotem>();
		QSBWorldSync.Init<QSBAlarmBell, AlarmBell>();
	}
}
