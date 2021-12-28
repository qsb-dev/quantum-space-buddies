using QSB.WorldSync;
using QSB.ZeroGCaveSync.WorldObjects;

namespace QSB.ZeroGCaveSync
{
	internal class ZeroGCaveManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBSatelliteNode, SatelliteNode>(this);
	}
}
