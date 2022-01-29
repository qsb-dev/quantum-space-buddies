using Cysharp.Threading.Tasks;
using QSB.CampfireSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.CampfireSync
{
	internal class CampfireManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
			=> QSBWorldSync.Init<QSBCampfire, Campfire>();
	}
}
