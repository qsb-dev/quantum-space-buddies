using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
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

		// BUG : This plays the sound to everyone
		// TODO : ehhhh idk about this. maybe copy each sound source so we have a 2d version (for local) and a 3d version (for remote)?
		// this would probably be a whole qsb version on it's own

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ProbeLauncherEffects), nameof(ProbeLauncherEffects.PlayRetrievalClip))]
		public static bool ProbeLauncherEffects_PlayRetrievalClip(OWAudioSource ____owAudioSource)
		{
			____owAudioSource.GetAudioSource().spatialBlend = 1f;
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ProbeLauncherEffects), nameof(ProbeLauncherEffects.PlayLaunchClip))]
		public static bool ProbeLauncherEffects_PlayLaunchClip(OWAudioSource ____owAudioSource)
		{
			____owAudioSource.GetAudioSource().spatialBlend = 1f;
			return true;
		}
	}
}
