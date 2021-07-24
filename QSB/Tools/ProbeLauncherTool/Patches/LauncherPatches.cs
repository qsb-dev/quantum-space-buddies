using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool.Patches
{
	class LauncherPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			Prefix(nameof(ProbeLauncher_RetrieveProbe));
		}

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
			if (__instance == QSBPlayerManager.LocalPlayer.LocalProbeLauncher)
			{
				return true;
			}

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

				DebugLog.DebugWrite($"{__instance.name} retrieve probe playEffects:{playEffects}");

				QSBEventManager.FireEvent(EventNames.QSBRetrieveProbe, QSBWorldSync.GetWorldFromUnity<QSBProbeLauncher, ProbeLauncher>(__instance), playEffects);
				____activeProbe.Retrieve(____probeRetrievalLength);
				____isRetrieving = true;
			}

			return false;
		}
	}
}
