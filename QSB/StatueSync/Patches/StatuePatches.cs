using QSB.Events;
using QSB.Patches;
using QSB.Player;
using UnityEngine;

namespace QSB.StatueSync.Patches
{
	internal class StatuePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
			=> Prefix(nameof(MemoryUplinkTrigger_Update));

		public static bool MemoryUplinkTrigger_Update(bool ____waitForPlayerGrounded)
		{
			if (StatueManager.Instance.HasStartedStatueLocally)
			{
				return true;
			}

			if (!____waitForPlayerGrounded || !Locator.GetPlayerController().IsGrounded())
			{
				return true;
			}

			var playerBody = Locator.GetPlayerBody().transform;
			var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
			QSBEventManager.FireEvent(
				EventNames.QSBStartStatue,
				timberHearth.InverseTransformPoint(playerBody.position),
				Quaternion.Inverse(timberHearth.rotation) * playerBody.rotation,
				Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().GetDegreesY());
			QSBPlayerManager.HideAllPlayers();
			return true;
		}
	}
}
