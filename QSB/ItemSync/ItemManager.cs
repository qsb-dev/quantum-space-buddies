using Cysharp.Threading.Tasks;
using OWML.Common;
using QSB.ItemSync.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.ItemSync.WorldObjects.Sockets;
using QSB.Utility;
using QSB.Utility.Deterministic;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
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
		// QSBWorldSync.Init<QSBScrollItem, ScrollItem>();
		QSBWorldSync.Init<QSBSharedStone, SharedStone>();
		QSBWorldSync.Init<QSBSimpleLanternItem, SimpleLanternItem>();
		QSBWorldSync.Init<QSBSlideReelItem, SlideReelItem>();
		QSBWorldSync.Init<QSBWarpCoreItem, WarpCoreItem>();
        // dream lantern and vision torch are set up in their own managers

        // Use the basic QSBItem class for any items that do not require custom code through a derived class (mod compatibility)
        // QSB addons can still define their own QSBItem derived classes and they will just get skipped here
        var handledItemTypes = GetHandledItemTypes();
        DebugLog.DebugWrite($"Handled OWItem types (the rest will get generic QSBItem support) are: {string.Join(", ", handledItemTypes)}"); 
        var otherItemlistToInitFrom = QSBWorldSync.GetUnityObjects<OWItem>()
            .Where(x => !handledItemTypes.Contains(x.GetType()))
            .SortDeterministic();
        QSBWorldSync.Init<QSBItem<OWItem>, OWItem>(otherItemlistToInitFrom);

        // Sockets
        QSBWorldSync.Init<QSBItemSocket, OWItemSocket>();

		// other drop targets that don't already have world objects
		var listToInitFrom = QSBWorldSync.GetUnityObjects<MonoBehaviour>()
			.Where(x => x is IItemDropTarget and not (RaftDock or RaftController or PrisonCellElevator))
			.SortDeterministic();
		QSBWorldSync.Init<QSBOtherDropTarget, MonoBehaviour>(listToInitFrom);
	}

    /// <summary>
    /// Gets all types that extend QSBItem and returns the list of OWItem types that are already handled by dedicated classes
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<Type> GetHandledItemTypes()
    {
        var assemblies = QSBCore.Addons.Values
            .Select(x => x.GetType().Assembly)
            .Append(typeof(QSBCore).Assembly);

        if (QSBCore.QSBNHAssembly != null)
        {
            assemblies = assemblies.Append(QSBCore.QSBNHAssembly);
        }

        return assemblies.SelectMany(x => x.GetTypes())
            .Where(x => !x.IsInterface && !x.IsAbstract && x.BaseType != null && x.BaseType.IsGenericType && x.BaseType.GetGenericTypeDefinition() == typeof(QSBItem<>))
            .Select(x => x.BaseType.GetGenericArguments()[0])
            .OrderBy(x => x.FullName);
    }
}
