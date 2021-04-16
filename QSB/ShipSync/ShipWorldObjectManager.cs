using OWML.Common;
using QSB.ShipSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ShipSync
{
	public class ShipWorldObjectManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding ship object...", MessageType.Warning);
			QSBWorldSync.Init<QSBShip, ShipBody>();
		}
	}
}
