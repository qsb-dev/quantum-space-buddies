using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Threading;

namespace QSB.EchoesOfTheEye.RaftSync;

public class RaftManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public static readonly List<RaftController> Rafts = new();

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		Rafts.Clear();
		Rafts.AddRange(QSBWorldSync.GetUnityObjects<RaftController>().SortDeterministic());
		QSBWorldSync.Init<QSBRaft, RaftController>(Rafts);
	}
}
