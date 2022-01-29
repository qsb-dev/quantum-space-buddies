using Cysharp.Threading.Tasks;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Threading;

namespace QSB.OrbSync
{
	public class OrbManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public static readonly List<NomaiInterfaceOrb> Orbs = new();

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		{
			Orbs.Clear();
			Orbs.AddRange(QSBWorldSync.GetUnityObjects<NomaiInterfaceOrb>().SortDeterministic());
			QSBWorldSync.Init<QSBOrb, NomaiInterfaceOrb>(Orbs);
		}
	}
}
