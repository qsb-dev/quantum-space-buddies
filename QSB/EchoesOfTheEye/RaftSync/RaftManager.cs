using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.RaftSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QSB.EchoesOfTheEye.RaftSync;

public class RaftManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public static readonly List<RaftController> Rafts = new();
	public static DamRaftLift DamRaftLift { get; private set; }

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		Rafts.Clear();
		Rafts.AddRange(QSBWorldSync.GetUnityObjects<RaftController>().SortDeterministic());
		QSBWorldSync.Init<QSBRaft, RaftController>(Rafts);

		QSBWorldSync.Init<QSBRaftDock, RaftDock>();
		DamRaftLift = QSBWorldSync.GetUnityObjects<DamRaftLift>().First();
	}
}
