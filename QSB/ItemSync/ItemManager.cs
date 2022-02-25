using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;

namespace QSB.ItemSync;

internal class ItemManager : WorldObjectManager
{
	public override WorldObjectType WorldObjectType => WorldObjectType.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		DebugLog.DebugWrite("Building OWItems...", MessageType.Info);

		// Items
		QSBWorldSync.Init<QSBDreamLanternItem, DreamLanternItem>();
		QSBWorldSync.Init<QSBNomaiConversationStone, NomaiConversationStone>();
		QSBWorldSync.Init<QSBScrollItem, ScrollItem>();
		QSBWorldSync.Init<QSBSharedStone, SharedStone>();
		QSBWorldSync.Init<QSBSimpleLanternItem, SimpleLanternItem>();
		QSBWorldSync.Init<QSBSlideReelItem, SlideReelItem>();
		QSBWorldSync.Init<QSBVisionTorchItem, VisionTorchItem>();
		QSBWorldSync.Init<QSBWarpCoreItem, WarpCoreItem>();

		// Sockets
		QSBWorldSync.Init<QSBItemSocket, OWItemSocket>();
	}
}