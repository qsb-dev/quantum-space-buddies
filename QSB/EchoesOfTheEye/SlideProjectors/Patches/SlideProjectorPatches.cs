using HarmonyLib;
using QSB.EchoesOfTheEye.SlideProjectors.Messages;
using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.SlideProjectors.Patches;

[HarmonyPatch(typeof(SlideProjector))]
public class SlideProjectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SlideProjector.OnPressInteract))]
	public static void OnPressInteract(SlideProjector __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}
		__instance.GetWorldObject<QSBSlideProjector>().SendMessage(new UseSlideProjectorMessage(true));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SlideProjector.CancelInteraction))]
	public static void CancelInteraction(SlideProjector __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}
		__instance.GetWorldObject<QSBSlideProjector>().SendMessage(new UseSlideProjectorMessage(false));
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SlideProjector.NextSlide))]
	public static void NextSlide(SlideProjector __instance)
	{
		if (Remote)
		{
			return;
		}
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}
		__instance.GetWorldObject<QSBSlideProjector>().SendMessage(new NextSlideMessage());
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(SlideProjector.PreviousSlide))]
	public static void PreviousSlide(SlideProjector __instance)
	{
		if (Remote)
		{
			return;
		}
		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}
		__instance.GetWorldObject<QSBSlideProjector>().SendMessage(new PreviousSlideMessage());
	}
}
