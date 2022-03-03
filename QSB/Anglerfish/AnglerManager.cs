using Cysharp.Threading.Tasks;
using QSB.Anglerfish.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Threading;

namespace QSB.Anglerfish;

public class AnglerManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public static readonly List<AnglerfishController> Anglers = new();

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		Anglers.Clear();
		Anglers.AddRange(QSBWorldSync.GetUnityObjects<AnglerfishController>().SortDeterministic());
		QSBWorldSync.Init<QSBAngler, AnglerfishController>(Anglers);
	}
}