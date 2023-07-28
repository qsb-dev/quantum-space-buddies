using HarmonyLib;
using OWML.Utils;
using QSB.Patches;
using UnityEngine.UI;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(TitleScreenManager))]
public class TitleScreenManagerPatchesGamepass : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;
	public override GameVendor PatchVendor => GameVendor.Gamepass;

	[HarmonyPrefix]
	[HarmonyPatch("SetUserAccountDisplayInfo")]
	public static bool SetUserAccountDisplayInfo(TitleScreenManager __instance)
	{
		var text = __instance.GetValue<Text>("_gamertagDisplay");
		text.text = ""; // no idea why, mobius be like
		text.text = QSBMSStoreProfileManager.SharedInstance.userDisplayName;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(nameof(TitleScreenManager.InitializeProfileManagerCallbacks))]
	public static bool InitializeProfileManagerCallbacks(TitleScreenManager __instance)
	{
		QSBMSStoreProfileManager.SharedInstance.OnBrokenDataExists += __instance.OnBrokenDataExists;

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
		QSBMSStoreProfileManager.SharedInstance.OnBrokenDataExists -= __instance.OnBrokenDataExists;

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
