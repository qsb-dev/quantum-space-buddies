using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AlarmTotemSync;

public class AlarmTotemManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	private QSBAlarmSequenceController _qsbAlarmSequenceController;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		// QSBWorldSync.Init<QSBAlarmTotem, AlarmTotem>();
		QSBWorldSync.Init<QSBAlarmBell, AlarmBell>();

		_qsbAlarmSequenceController = new GameObject(nameof(QSBAlarmSequenceController))
			.AddComponent<QSBAlarmSequenceController>();
		DontDestroyOnLoad(_qsbAlarmSequenceController.gameObject);
	}

	public override void UnbuildWorldObjects() =>
		Destroy(_qsbAlarmSequenceController.gameObject);
}
