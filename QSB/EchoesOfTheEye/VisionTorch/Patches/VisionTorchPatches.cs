using HarmonyLib;
using QSB.EchoesOfTheEye.VisionTorch.Messages;
using QSB.EchoesOfTheEye.VisionTorch.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.VisionTorch.Patches;

public class VisionTorchPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(VisionTorchItem), nameof(VisionTorchItem.Update))]
	private static bool Update(VisionTorchItem __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (PlayerState.IsViewingProjector() && __instance._mindSlideProjector.mindSlideCollection.slideCollectionContainer.slideIndex == 1)
		{
			OWInput.ChangeInputMode(InputMode.None);
			__instance._mindSlideProjector.OnProjectionComplete += __instance.OnProjectionComplete;
			__instance.enabled = false;
			return false;
		}
		__instance._wasProjecting = __instance._isProjecting;
		__instance._isProjecting = OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character);
		if (__instance._isProjecting && !__instance._wasProjecting)
		{
			__instance._mindProjectorTrigger.SetProjectorActive(true);
			__instance.GetWorldObject<QSBVisionTorchItem>().SendMessage(new VisionTorchProjectMessage(true));
		}
		else if (!__instance._isProjecting && __instance._wasProjecting)
		{
			__instance._mindProjectorTrigger.SetProjectorActive(false);
			__instance.GetWorldObject<QSBVisionTorchItem>().SendMessage(new VisionTorchProjectMessage(false));
		}

		return false;
	}
}
