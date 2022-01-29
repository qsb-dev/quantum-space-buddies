using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using QSB.ZeroGCaveSync.WorldObjects;
using System.Threading;

namespace QSB.ZeroGCaveSync
{
	internal class ZeroGCaveManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken cancellationToken)
			=> QSBWorldSync.Init<QSBSatelliteNode, SatelliteNode>();
	}
}
