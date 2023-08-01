using HarmonyLib;
using QSB.Patches;

namespace QSB.SaveSync.Patches;

[HarmonyPatch(typeof(TitleScreenManager))]
public class TitleScreenManagerPatchesCommon : QSBPatch
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
}
