using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.StatueSync.Messages;
using UnityEngine;

namespace QSB.StatueSync.Patches;

[HarmonyPatch]
public class StatuePatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MemoryUplinkTrigger), nameof(MemoryUplinkTrigger.Update))]
	public static bool MemoryUplinkTrigger_Update(MemoryUplinkTrigger __instance)
	{
		if (StatueManager.Instance.HasStartedStatueLocally)
		{
			return true;
		}

		if (!__instance._waitForPlayerGrounded || !Locator.GetPlayerController().IsGrounded())
		{
			return true;
		}

		var playerBody = Locator.GetPlayerBody().transform;
		var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
		new StartStatueMessage(
			timberHearth.InverseTransformPoint(playerBody.position),
			Quaternion.Inverse(timberHearth.rotation) * playerBody.rotation,
			Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().GetDegreesY()
		).Send();
		QSBPlayerManager.HideAllPlayers();
		return true;
	}
}