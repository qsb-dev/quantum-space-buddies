using HarmonyLib;
using QSB.EchoesOfTheEye.SlideProjectors.Messages;
using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.SlideProjectors
{
	internal class ProjectorPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.OnPressInteract))]
		public static void Interact(SlideProjector __instance)
		{
			var worldObject = QSBWorldSync.GetWorldObject<QSBSlideProjector>(__instance);
			worldObject.SendMessage(new ProjectorAuthorityMessage(QSBPlayerManager.LocalPlayerId));
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.CancelInteraction))]
		public static void CancelInteract(SlideProjector __instance)
		{
			var worldObject = QSBWorldSync.GetWorldObject<QSBSlideProjector>(__instance);
			worldObject.SendMessage(new ProjectorAuthorityMessage(0u));
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.NextSlide))]
		public static void NextSlide(SlideProjector __instance)
		{
			var worldObject = QSBWorldSync.GetWorldObject<QSBSlideProjector>(__instance);
			worldObject.SendMessage(new NextSlideMessage());
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(SlideProjector), nameof(SlideProjector.PreviousSlide))]
		public static void PreviousSlide(SlideProjector __instance)
		{
			var worldObject = QSBWorldSync.GetWorldObject<QSBSlideProjector>(__instance);
			worldObject.SendMessage(new PreviousSlideMessage());
		}
	}
}
