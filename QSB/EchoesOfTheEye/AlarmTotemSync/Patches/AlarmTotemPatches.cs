using HarmonyLib;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Patches;

public class AlarmTotemPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AlarmTotem), nameof(AlarmTotem.SetFaceOpen))]
	private static void SetFaceOpen(AlarmTotem __instance, bool open)
	{
		if (Remote)
		{
			return;
		}

		if (__instance._isFaceOpen == open)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		__instance.GetWorldObject<QSBAlarmTotem>()
			.SendMessage(new SetFaceOpenMessage(open));
	}
}
