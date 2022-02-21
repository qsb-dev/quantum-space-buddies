using HarmonyLib;
using QSB.EchoesOfTheEye.SlideProjectors.Messages;
using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.SlideProjectors.Patches
{
	internal class ProjectorPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.OnPressInteract))]
		public static void Interact(SlideProjector __instance) =>
			__instance.GetWorldObject<QSBSlideProjector>()
				.SendMessage(new ProjectorAuthorityMessage(QSBPlayerManager.LocalPlayerId));

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.CancelInteraction))]
		public static void CancelInteract(SlideProjector __instance) =>
			__instance.GetWorldObject<QSBSlideProjector>()
				.SendMessage(new ProjectorAuthorityMessage(0));

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.NextSlide))]
		public static void NextSlide(SlideProjector __instance) =>
			__instance.GetWorldObject<QSBSlideProjector>()
				.SendMessage(new NextSlideMessage());

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.PreviousSlide))]
		public static void PreviousSlide(SlideProjector __instance) =>
			__instance.GetWorldObject<QSBSlideProjector>()
				.SendMessage(new PreviousSlideMessage());
	}
}
