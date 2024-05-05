using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Utility.Deterministic;
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
		QSBWorldSync.Init<QSBAlarmBell, AlarmBell>();
		AlarmBells = QSBWorldSync.GetUnityObjects<AlarmBell>().Where(x => x._lightController).SortDeterministic().ToArray();
	}
}
