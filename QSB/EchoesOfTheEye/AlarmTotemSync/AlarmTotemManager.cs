using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.EchoesOfTheEye.AlarmTotemSync;

public class AlarmTotemManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public static AlarmBell[] AlarmBells;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBAlarmTotem, AlarmTotem>();
		AlarmBells = QSBWorldSync.GetUnityObjects<AlarmBell>()
			.Where(x => x._oneShotSource && x._animation && x._bellTrigger && x._lightController)
			.ToArray();
		QSBWorldSync.Init<QSBAlarmBell, AlarmBell>(AlarmBells);
	}
}
