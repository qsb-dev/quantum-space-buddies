using HarmonyLib;
using QSB.EchoesOfTheEye.DreamWorld.Messages;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.DreamWorld.Patches;

internal class DreamWorldPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(DreamWorldController), nameof(DreamWorldController.SpawnInDreamWorld))]
	public static void EnterDreamWorld()
	{
		var currentLantern = (DreamLanternItem)Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
		var currentQSBLantern = currentLantern.GetWorldObject<QSBDreamLanternItem>();
		new EnterDreamWorldMessage(currentQSBLantern.ObjectId).Send();
	}
}
