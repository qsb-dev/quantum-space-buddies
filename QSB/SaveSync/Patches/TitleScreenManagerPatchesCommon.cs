using HarmonyLib;
using QSB.Patches;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(TitleScreenManager))]
internal class TitleScreenManagerPatchesCommon : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(TitleScreenManager.Awake))]
	public static bool Awake(TitleScreenManager __instance)
	{
		__instance._profileManager = QSBCore.ProfileManager;
		__instance._profileManager.PreInitialize();
		LoadManager.OnStartSceneLoad += __instance.OnStartSceneLoad;
		LoadManager.OnCompleteSceneLoad += __instance.OnCompleteSceneLoad;
		MenuStackManager.SharedInstance.OnMenuPush += __instance.OnMenuPush;
		MenuStackManager.SharedInstance.OnMenuPop += __instance.OnMenuPop;
		__instance._resumeGameTextSetter = __instance._resumeGameObject.GetComponentInChildren<ResumeGameLocalizedText>();
		__instance.InitializePopupPrompts();

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(TitleScreenManager.InitializeProfileManagerCallbacks))]
	public static bool InitializeProfileManagerCallbacks(TitleScreenManager __instance)
	{
		if (QSBCore.IsStandalone)
		{
			QSBStandaloneProfileManager.SharedInstance.OnNoProfilesExist += __instance.OnNoStandaloneProfilesExist;
			QSBStandaloneProfileManager.SharedInstance.OnUpdatePlayerProfiles += __instance.OnUpdatePlayerProfiles;
			QSBStandaloneProfileManager.SharedInstance.OnBrokenDataExists += __instance.OnBrokenDataExists;
		}
		else
		{
			QSBMSStoreProfileManager.SharedInstance.OnBrokenDataExists += __instance.OnBrokenDataExists;
		}

		__instance._profileManager.OnProfileSignInStart += __instance.OnProfileSignInStart;
		__instance._profileManager.OnProfileSignInComplete += __instance.OnProfileSignInComplete;
		__instance._profileManager.OnProfileSignOutStart += __instance.OnProfileSignOutStart;
		__instance._profileManager.OnProfileSignOutComplete += __instance.OnProfileSignOutComplete;
		__instance._profileManager.OnProfileReadDone += __instance.OnProfileManagerReadDone;
		__instance._profileManager.Initialize();

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(TitleScreenManager.OnDestroy))]
	public static bool OnDestroy(TitleScreenManager __instance)
	{
		if (QSBCore.IsStandalone)
		{
			QSBStandaloneProfileManager.SharedInstance.OnNoProfilesExist -= __instance.OnNoStandaloneProfilesExist;
			QSBStandaloneProfileManager.SharedInstance.OnUpdatePlayerProfiles -= __instance.OnUpdatePlayerProfiles;
			QSBStandaloneProfileManager.SharedInstance.OnBrokenDataExists -= __instance.OnBrokenDataExists;
		}
		else
		{
			QSBMSStoreProfileManager.SharedInstance.OnBrokenDataExists -= __instance.OnBrokenDataExists;
		}

		__instance._profileManager.OnProfileSignInStart -= __instance.OnProfileSignInStart;
		__instance._profileManager.OnProfileSignInComplete -= __instance.OnProfileSignInComplete;
		__instance._profileManager.OnProfileSignOutStart -= __instance.OnProfileSignOutStart;
		__instance._profileManager.OnProfileSignOutComplete -= __instance.OnProfileSignOutComplete;
		__instance._profileManager.OnProfileReadDone -= __instance.OnProfileManagerReadDone;
		LoadManager.OnStartSceneLoad -= __instance.OnStartSceneLoad;
		LoadManager.OnCompleteSceneLoad -= __instance.OnCompleteSceneLoad;
		TextTranslation.Get().OnLanguageChanged -= __instance.OnLanguageChanged;
		__instance._newGameAction.OnSubmitAction -= __instance.OnNewGameSubmit;
		__instance._newGameAction.OnPostSetupPopup -= __instance.OnNewGameSetupPopup;
		__instance._resetGameAction.OnSubmitAction -= __instance.OnResetGameSubmit;
		__instance._accountPickerSubmitAction.OnAccountPickerSubmitEvent -= __instance.OnAccountPickerSubmitEvent;
		MenuStackManager.SharedInstance.OnMenuPush -= __instance.OnMenuPush;
		MenuStackManager.SharedInstance.OnMenuPop -= __instance.OnMenuPop;

		return false;
	}
}
