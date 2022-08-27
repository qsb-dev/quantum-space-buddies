using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.ProbeLauncherTool.Messages;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool.Patches;

[HarmonyPatch]
internal class LauncherPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProbeLauncher), nameof(ProbeLauncher.RetrieveProbe))]
	public static bool ProbeLauncher_RetrieveProbe(
		ProbeLauncher __instance,
		bool playEffects,
		bool forcedRetrieval)
	{

		if (__instance._isRetrieving)
		{
			return false;
		}

		if (__instance._activeProbe != null)
		{
			if (__instance._activeProbe.IsLaunched() && TimelineObliterationController.IsParadoxProbeActive() && !forcedRetrieval)
			{
				var data = new NotificationData(__instance._notificationFilter, UITextLibrary.GetString(UITextType.NotificationMultProbe), 3f);
				NotificationManager.SharedInstance.PostNotification(data);
				Locator.GetPlayerAudioController().PlayNegativeUISound();
				return false;
			}

			__instance._activeProbe.GetRotatingCamera().ResetRotation();
			__instance._preLaunchProbeProxy.SetActive(true);
			if (playEffects)
			{
				__instance._effects.PlayRetrievalClip();
				__instance._probeRetrievalEffect.WarpObjectIn(__instance._probeRetrievalLength);
			}

			if (__instance != QSBPlayerManager.LocalPlayer.LocalProbeLauncher)
			{
				__instance.GetWorldObject<QSBProbeLauncher>()
					.SendMessage(new RetrieveProbeMessage(playEffects));
			}
			else
			{
				new PlayerRetrieveProbeMessage(playEffects).Send();
			}

			__instance._activeProbe.Retrieve(__instance._probeRetrievalLength);
			__instance._isRetrieving = true;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ProbeLauncherEffects), nameof(ProbeLauncherEffects.PlayRetrievalClip))]
	public static bool ProbeLauncherEffects_PlayRetrievalClip(ProbeLauncherEffects __instance)
	{
		if (__instance._owAudioSource == null)
		{
			DebugLog.ToConsole($"Error - _owAudioSource of {__instance.name} is null.", OWML.Common.MessageType.Error);
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
			DebugLog.ToConsole($"Error - _owAudioSource of {__instance.name} is null.", OWML.Common.MessageType.Error);
			return true;
		}

		__instance._owAudioSource.GetAudioSource().spatialBlend = 1f;
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ProbeLauncherEffects), nameof(ProbeLauncherEffects.PlayChangeModeClip))]
	public static void ProbeLauncherEffects_PlayChangeModeClip(ProbeLauncherEffects __instance)
	{
		if (__instance != QSBPlayerManager.LocalPlayer.LocalProbeLauncher._effects)
		{
			__instance.gameObject.GetComponent<ProbeLauncher>().GetWorldObject<QSBProbeLauncher>()
				.SendMessage(new ChangeModeMessage());
		}
		else
		{
			new PlayerLauncherChangeModeMessage().Send();
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ProbeLauncherEffects), nameof(ProbeLauncherEffects.PlaySnapshotClip))]
	public static void ProbeLauncherEffects_PlaySnapshotClip(ProbeLauncherEffects __instance)
	{
		if (__instance != QSBPlayerManager.LocalPlayer.LocalProbeLauncher._effects)
		{
			__instance.gameObject.GetComponent<ProbeLauncher>().GetWorldObject<QSBProbeLauncher>()
				.SendMessage(new TakeSnapshotMessage());
		}
		else
		{
			new PlayerLauncherTakeSnapshotMessage().Send();
		}
	}
}