using Cysharp.Threading.Tasks;
using QSB.Utility;
using System.Linq;
using System.Threading;

namespace QSB.WorldSync;

public class MiscManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;
	public override bool DlcOnly => false;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		await UniTask.WaitUntil(() => LateInitializerManager.isDoneInitializing, cancellationToken: ct);

		var listToInitFrom = QSBWorldSync.GetUnityObjects<OWRigidbody>()
			.Where(x =>
				x is not (PlayerBody or ShipBody or ShuttleBody) &&
				!x.TryGetComponent<SurveyorProbe>(out _)
			)
			.SortDeterministic();
		QSBWorldSync.Init<QSBOWRigidbody, OWRigidbody>(listToInitFrom);
	}
}
