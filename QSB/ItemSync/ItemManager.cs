using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.ItemSync.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Utility;
using QSB.Utility.Deterministic;
using QSB.WorldSync;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.ItemSync;

public class ItemManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		DebugLog.DebugWrite("Building OWItems...", MessageType.Info);

		// Items
		QSBWorldSync.Init<QSBNomaiConversationStone, NomaiConversationStone>();
		QSBWorldSync.Init<QSBScrollItem, ScrollItem>();
		QSBWorldSync.Init<QSBSharedStone, SharedStone>();
		QSBWorldSync.Init<QSBSimpleLanternItem, SimpleLanternItem>();
		QSBWorldSync.Init<QSBSlideReelItem, SlideReelItem>();
		QSBWorldSync.Init<QSBWarpCoreItem, WarpCoreItem>();
		// dream lantern and vision torch are set up in their own managers

		// Sockets
		QSBWorldSync.Init<QSBItemSocket, OWItemSocket>();

		// other drop targets that don't already have world objects
		var listToInitFrom = QSBWorldSync.GetUnityObjects<MonoBehaviour>()
			.Where(x => x is IItemDropTarget and not (RaftDock or RaftController or PrisonCellElevator))
			.SortDeterministic();
		QSBWorldSync.Init<QSBOtherDropTarget, MonoBehaviour>(listToInitFrom);
	}
}
