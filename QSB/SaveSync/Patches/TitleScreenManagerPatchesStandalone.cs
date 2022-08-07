using HarmonyLib;
using QSB.Patches;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(TitleScreenManager))]
internal class TitleScreenManagerPatchesStandalone : QSBPatch
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
}
