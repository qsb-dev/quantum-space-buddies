using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Tools.TranslatorTool.TranslationSync.Messages;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Tools.TranslatorTool.TranslationSync.Patches;

public class SpiralPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiText), nameof(NomaiText.SetAsTranslated))]
	public static void SetAsTranslated(NomaiText __instance, int id)
	{
		if (__instance is GhostWallText)
		{
			return;
		}

		if (__instance.IsTranslated(id))
		{
			return;
		}

		__instance.GetWorldObject<QSBNomaiText>()
			.SendMessage(new SetAsTranslatedMessage(id));
	}
}