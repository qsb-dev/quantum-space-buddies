using HarmonyLib;
using QSB.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(InGameProfileMenuManager))]
internal class InGameProfileMenuManagerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(InGameProfileMenuManager.InitializeOnAwake))]
	public static bool InitializeOnAwake(InGameProfileMenuManager __instance)
	{
		if (!__instance._initialized)
		{
			TextTranslation.Get().OnLanguageChanged += __instance.UpdateLanguage;
			__instance.UpdateLanguage();
			__instance._profileManager = QSBCore.ProfileManager;
			__instance._profileManager.OnProfileSignInComplete += __instance.OnProfileSignInComplete;
			__instance._profileManager.OnProfileSignOutComplete += __instance.OnProfileSignOutComplete;
			__instance._profileManager.OnProfileReadDone += __instance.OnProfileReadDone;
			__instance._returnToGameSubmitAction.OnSubmitAction += __instance.OnResumeGameBtnSubmit;
			__instance._returnToTitleSubmitAction.OnSubmitAction += __instance.OnTitleSubmitAction;
			LoadManager.OnStartSceneLoad += __instance.OnStartSceneLoad;
			LoadManager.OnCompleteSceneLoad += __instance.OnCompleteSceneLoad;
			GlobalMessenger.AddListener("PlayerResurrection", new Callback(__instance.OnPlayerResurrection));
			__instance._initialized = true;
		}

		return false;
	}
}
