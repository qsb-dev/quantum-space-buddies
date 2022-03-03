using HarmonyLib;
using QSB.EchoesOfTheEye.SlideProjectors.Messages;
using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.SlideProjectors.Patches;

[HarmonyPatch(typeof(SlideProjector))]
internal class SlideProjectorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPostfix]
	[HarmonyPatch(nameof(SlideProjector.OnPressInteract))]
	public static void OnPressInteract(SlideProjector __instance) =>
		__instance.GetWorldObject<QSBSlideProjector>()
			.SendMessage(new UseSlideProjectorMessage(true));

	[HarmonyPostfix]
	[HarmonyPatch(nameof(SlideProjector.CancelInteraction))]
	public static void CancelInteraction(SlideProjector __instance) =>
		__instance.GetWorldObject<QSBSlideProjector>()
			.SendMessage(new UseSlideProjectorMessage(false));

	[HarmonyPostfix]
	[HarmonyPatch(nameof(SlideProjector.NextSlide))]
	public static void NextSlide(SlideProjector __instance) =>
		__instance.GetWorldObject<QSBSlideProjector>()
			.SendMessage(new NextSlideMessage());

	[HarmonyPostfix]
	[HarmonyPatch(nameof(SlideProjector.PreviousSlide))]
	public static void PreviousSlide(SlideProjector __instance) =>
		__instance.GetWorldObject<QSBSlideProjector>()
			.SendMessage(new PreviousSlideMessage());
}
