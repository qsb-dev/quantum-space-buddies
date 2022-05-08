using HarmonyLib;
using QSB.EchoesOfTheEye.DreamCandles.Messages;
using QSB.EchoesOfTheEye.DreamCandles.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.DreamCandles.Patches;

public class DreamCandlePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	public static bool DontSendMessage;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DreamCandle), nameof(DreamCandle.SetLit))]
	private static void SetLit(DreamCandle __instance,
		bool lit, bool playAudio, bool instant)
	{
		if (Remote)
		{
			return;
		}

		if (lit == __instance._lit)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		if (DontSendMessage)
		{
			return;
		}

		__instance.GetWorldObject<QSBDreamCandle>()
			.SendMessage(new SetLitMessage(lit, playAudio, instant));
	}
}
