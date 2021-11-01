using OWML.Common;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync
{
	internal class ItemManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding OWItems...", MessageType.Info);

			// Items
			QSBWorldSync.Init<QSBScrollItem, ScrollItem>();
			QSBWorldSync.Init<QSBSharedStone, SharedStone>();
			QSBWorldSync.Init<QSBWarpCoreItem, WarpCoreItem>();
			QSBWorldSync.Init<QSBNomaiConversationStone, NomaiConversationStone>();
			QSBWorldSync.Init<QSBSimpleLanternItem, SimpleLanternItem>();

			// Sockets
			QSBWorldSync.Init<QSBScrollSocket, ScrollSocket>();
			QSBWorldSync.Init<QSBSharedStoneSocket, SharedStoneSocket>();
			QSBWorldSync.Init<QSBWarpCoreSocket, WarpCoreSocket>();
			QSBWorldSync.Init<QSBNomaiConversationStoneSocket, NomaiConversationStoneSocket>();
		}
	}
}
