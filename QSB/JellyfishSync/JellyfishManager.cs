using Cysharp.Threading.Tasks;
using QSB.JellyfishSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Threading;

namespace QSB.JellyfishSync
{
	public class JellyfishManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public static readonly List<JellyfishController> Jellyfish = new();

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		{
			Jellyfish.Clear();
			Jellyfish.AddRange(QSBWorldSync.GetUnityObjects<JellyfishController>().SortDeterministic());
			QSBWorldSync.Init<QSBJellyfish, JellyfishController>(Jellyfish);
		}
	}
}
