using HarmonyLib;
using QSB.Patches;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.Animation.NPC.Patches
{
	[HarmonyPatch(typeof(TravelerController))]
	public class TravelerControllerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(nameof(TravelerController.OnStartConversation))]
		public static bool OnStartConversation(TravelerController __instance)
		{
			DebugLog.DebugWrite($"{__instance.name} OnStartConversation");
			__instance._talking = true;

			if (__instance._animator != null && __instance._animator.enabled)
			{
				__instance._playingAnimID = __instance._animator.IsInTransition(0)
					? __instance._animator.GetNextAnimatorStateInfo(0).fullPathHash
					: __instance._animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

				__instance._animator.SetTrigger("Talking");
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(TravelerController.OnEndConversation))]
		public static bool OnEndConversation(TravelerController __instance)
		{
			DebugLog.DebugWrite($"{__instance.name} OnEndConversation");
			if (__instance._animator != null && __instance._animator.enabled)
			{
				if (__instance._delayToRestartAudio > 0f)
				{
					__instance._animator.CrossFadeInFixedTime(__instance._playingAnimID, __instance._delayToRestartAudio, -1, -__instance._delayToRestartAudio);
				}
				else
				{
					__instance._animator.SetTrigger("Playing");
				}
			}

			__instance._talking = false;

			return false;
		}
	}
}
