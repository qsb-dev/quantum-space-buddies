using QSB.Events;
using QSB.Patches;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.StatueSync.Patches
{
	class StatuePatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnServerClientConnect;

		public override void DoPatches() 
			=> QSBCore.Helper.HarmonyHelper.AddPrefix<MemoryUplinkTrigger>("Update", typeof(StatuePatches), nameof(Statue_Update));

		public override void DoUnpatches() 
			=> QSBCore.Helper.HarmonyHelper.Unpatch<MemoryUplinkTrigger>("BeginUplinkSequence");

		public static bool Statue_Update(bool ____waitForPlayerGrounded)
		{
			if (!____waitForPlayerGrounded || !Locator.GetPlayerController().IsGrounded())
			{
				return true;
			}
			DebugLog.DebugWrite("statue_beginuplinksequence");
			var playerBody = Locator.GetPlayerBody().transform;
			var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;
			QSBEventManager.FireEvent(
				EventNames.QSBStartStatue,
				timberHearth.InverseTransformPoint(playerBody.position), 
				Quaternion.Inverse(timberHearth.rotation) * playerBody.rotation, 
				Locator.GetPlayerCamera().GetComponent<PlayerCameraController>().GetDegreesY());
			return true;
		}
	}
}
