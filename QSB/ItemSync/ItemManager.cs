using OWML.Common;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ItemSync
{
	internal class ItemManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			DebugLog.DebugWrite("Rebuilding OWItems...", MessageType.Info);

			// Items
			QSBWorldSync.Init<QSBScrollItem, ScrollItem>(this);
			QSBWorldSync.Init<QSBSharedStone, SharedStone>(this);
			QSBWorldSync.Init<QSBWarpCoreItem, WarpCoreItem>(this);
			QSBWorldSync.Init<QSBNomaiConversationStone, NomaiConversationStone>(this);
			QSBWorldSync.Init<QSBSimpleLanternItem, SimpleLanternItem>(this);
			QSBWorldSync.Init<QSBSlideReelItem, SlideReelItem>(this);

			// Sockets
			QSBWorldSync.Init<QSBScrollSocket, ScrollSocket>(this);
			QSBWorldSync.Init<QSBSharedStoneSocket, SharedStoneSocket>(this);
			QSBWorldSync.Init<QSBWarpCoreSocket, WarpCoreSocket>(this);
			QSBWorldSync.Init<QSBNomaiConversationStoneSocket, NomaiConversationStoneSocket>(this);
			QSBWorldSync.Init<QSBSlideReelSocket, SlideReelSocket>(this);
			QSBWorldSync.Init<QSBSlideProjectorSocket, SlideProjectorSocket>(this);
		}
	}
}
