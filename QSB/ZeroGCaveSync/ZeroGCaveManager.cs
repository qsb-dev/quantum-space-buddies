using QSB.WorldSync;
using QSB.ZeroGCaveSync.WorldObjects;

namespace QSB.ZeroGCaveSync
{
	internal class ZeroGCaveManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBSatelliteNode, SatelliteNode>();
	}
}
