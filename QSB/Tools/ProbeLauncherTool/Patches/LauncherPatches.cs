using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool.Patches
{
	[HarmonyPatch]
	internal class LauncherPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.RetrieveProbe))]
		public static bool ProbeLauncher_RetrieveProbe(
			ProbeLauncher __instance,
			bool playEffects,
			bool forcedRetrieval,
			ref bool ____isRetrieving,
			SurveyorProbe ____activeProbe,
			NotificationTarget ____notificationFilter,
			GameObject ____preLaunchProbeProxy,
			ProbeLauncherEffects ____effects,
			SingularityWarpEffect ____probeRetrievalEffect,
			float ____probeRetrievalLength)
		{

			if (____isRetrieving)
			{
				return false;
			}

			if (____activeProbe != null)
			{
				if (____activeProbe.IsLaunched() && TimelineObliterationController.IsParadoxProbeActive() && !forcedRetrieval)
				{
					var data = new NotificationData(____notificationFilter, UITextLibrary.GetString(UITextType.NotificationMultProbe), 3f, true);
					NotificationManager.SharedInstance.PostNotification(data, false);
					Locator.GetPlayerAudioController().PlayNegativeUISound();
					return false;
				}

				____activeProbe.GetRotatingCamera().ResetRotation();
				____preLaunchProbeProxy.SetActive(true);
				if (playEffects)
				{
					____effects.PlayRetrievalClip();
					____probeRetrievalEffect.WarpObjectIn(____probeRetrievalLength);
				}

				if (__instance != QSBPlayerManager.LocalPlayer.LocalProbeLauncher)
				{
					QSBEventManager.FireEvent(EventNames.QSBRetrieveProbe, __instance.GetWorldObject<QSBProbeLauncher>(), playEffects);
				}
				else
				{
					QSBEventManager.FireEvent(EventNames.QSBPlayerRetrieveProbe, playEffects);
				}

				____activeProbe.Retrieve(____probeRetrievalLength);
				____isRetrieving = true;
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ProbeLauncherEffects), nameof(ProbeLauncherEffects.PlayRetrievalClip))]
		public static bool ProbeLauncherEffects_PlayRetrievalClip(ProbeLauncherEffects __instance)
		{
			if (__instance._owAudioSource == null)
			{
				DebugLog.ToConsole($"Error - _owAudioSource of {__instance._owAudioSource}", OWML.Common.MessageType.Error);
				return true;
			}

			__instance._owAudioSource.GetAudioSource().spatialBlend = 1f;
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ProbeLauncherEffects), nameof(ProbeLauncherEffects.PlayLaunchClip))]
		public static bool ProbeLauncherEffects_PlayLaunchClip(ProbeLauncherEffects __instance)
		{
			if (__instance._owAudioSource == null)
			{
				DebugLog.ToConsole($"Error - _owAudioSource of {__instance._owAudioSource}", OWML.Common.MessageType.Error);
				return true;
			}

			__instance._owAudioSource.GetAudioSource().spatialBlend = 1f;
			return true;
		}
	}
}
