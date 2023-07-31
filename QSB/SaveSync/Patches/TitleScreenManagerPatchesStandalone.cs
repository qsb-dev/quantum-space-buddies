using HarmonyLib;
using QSB.Patches;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(TitleScreenManager))]
public class TitleScreenManagerPatchesStandalone : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;
	public override GameVendor PatchVendor => GameVendor.Epic | GameVendor.Steam;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(TitleScreenManager.OnBrokenDataExists))]
	public static bool OnBrokenDataExists(TitleScreenManager __instance)
	{
		__instance._titleMenuRaycastBlocker.blocksRaycasts = false;
		__instance._inputModule.EnableInputs();
		__instance._waitingOnBrokenDataResponse = true;
		var flag = QSBStandaloneProfileManager.SharedInstance.BackupExistsForBrokenData();
		var text = UITextLibrary.GetString(UITextType.SaveRestore_CorruptedMsg);
		if (flag)
		{
			text = text + " " + UITextLibrary.GetString(UITextType.SaveRestore_LoadPreviousMsg);
		}

		__instance._okCancelPopup.ResetPopup();
		__instance._okCancelPopup.SetUpPopup(text, InputLibrary.confirm, InputLibrary.cancel, __instance._confirmActionPrompt, __instance._cancelActionPrompt, true, flag);
		__instance._okCancelPopup.OnPopupConfirm += __instance.OnUserConfirmRestoreData;
		__instance._okCancelPopup.OnPopupCancel += __instance.OnUserCancelRestoreData;
		__instance._okCancelPopup.EnableMenu(true);
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(TitleScreenManager.OnUserConfirmRestoreData))]
	public static bool OnUserConfirmRestoreData(TitleScreenManager __instance)
	{
		__instance._waitingOnBrokenDataResponse = false;
		QSBStandaloneProfileManager.SharedInstance.RestoreCurrentProfileBackup();
		__instance.OnProfileManagerReadDone();
		__instance._okCancelPopup.OnPopupConfirm -= __instance.OnUserConfirmRestoreData;
		__instance._okCancelPopup.OnPopupCancel -= __instance.OnUserCancelRestoreData;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(TitleScreenManager.InitializeProfileManagerCallbacks))]
	public static bool InitializeProfileManagerCallbacks(TitleScreenManager __instance)
	{
		QSBStandaloneProfileManager.SharedInstance.OnNoProfilesExist += __instance.OnNoStandaloneProfilesExist;
		QSBStandaloneProfileManager.SharedInstance.OnUpdatePlayerProfiles += __instance.OnUpdatePlayerProfiles;
		QSBStandaloneProfileManager.SharedInstance.OnBrokenDataExists += __instance.OnBrokenDataExists;

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
		QSBStandaloneProfileManager.SharedInstance.OnNoProfilesExist -= __instance.OnNoStandaloneProfilesExist;
		QSBStandaloneProfileManager.SharedInstance.OnUpdatePlayerProfiles -= __instance.OnUpdatePlayerProfiles;
		QSBStandaloneProfileManager.SharedInstance.OnBrokenDataExists -= __instance.OnBrokenDataExists;

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
