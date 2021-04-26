using OWML.Common;
using QSB.Animation.Character.WorldObjects;
using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.ConversationSync.Patches
{
	public class CharacterAnimationPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.HarmonyHelper.AddPrefix<CharacterAnimController>("OnAnimatorIK", typeof(CharacterAnimationPatches), nameof(AnimController_OnAnimatorIK));
			QSBCore.HarmonyHelper.AddPrefix<CharacterAnimController>("OnZoneEntry", typeof(CharacterAnimationPatches), nameof(AnimController_OnZoneEntry));
			QSBCore.HarmonyHelper.AddPrefix<CharacterAnimController>("OnZoneExit", typeof(CharacterAnimationPatches), nameof(AnimController_OnZoneExit));
		}

		public override void DoUnpatches()
		{
			QSBCore.HarmonyHelper.Unpatch<CharacterAnimController>("OnAnimatorIK");
			QSBCore.HarmonyHelper.Unpatch<CharacterAnimController>("OnZoneExit");
		}

		public static bool AnimController_OnAnimatorIK(
			CharacterAnimController __instance,
			float ___headTrackingWeight,
			bool ___lookOnlyWhenTalking,
			bool ____playerInHeadZone,
			bool ____inConversation,
			ref float ____currentLookWeight,
			ref Vector3 ____currentLookTarget,
			DampedSpring3D ___lookSpring,
			Animator ____animator,
			CharacterDialogueTree ____dialogueTree)
		{

			var playerId = ConversationManager.Instance.GetPlayerTalkingToTree(____dialogueTree);

			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCharacterAnimController, CharacterAnimController>(__instance); // TODO : maybe cache this somewhere... or assess how slow this is

			Vector3 position;
			if (____inConversation)
			{
				if (playerId == uint.MaxValue)
				{
					DebugLog.DebugWrite($"Error - {__instance.name} is in conversation with a null player! Defaulting to active camera.", MessageType.Error);
					position = Locator.GetActiveCamera().transform.position;
				}
				else
				{
					var player = QSBPlayerManager.GetPlayer(playerId);
					position = player.CameraBody == null
						? Locator.GetActiveCamera().transform.position
						: player.CameraBody.transform.position;
				}
			}
			else if (!___lookOnlyWhenTalking && qsbObj.GetPlayersInHeadZone().Count != 0)
			{
				position = QSBPlayerManager.GetClosestPlayerToWorldPoint(qsbObj.GetPlayersInHeadZone(), __instance.transform.position).CameraBody.transform.position;
			}
			else
			{
				position = QSBPlayerManager.GetClosestPlayerToWorldPoint(__instance.transform.position, true).CameraBody.transform.position;
			}

			var localPosition = ____animator.transform.InverseTransformPoint(position);

			var targetWeight = ___headTrackingWeight;
			if (___lookOnlyWhenTalking)
			{
				if (!____inConversation || qsbObj.GetPlayersInHeadZone().Count == 0)
				{
					targetWeight *= 0;
				}
			}
			else
			{
				if (qsbObj.GetPlayersInHeadZone().Count == 0)
				{
					targetWeight *= 0;
				}
			}

			____currentLookWeight = Mathf.Lerp(____currentLookWeight, targetWeight, Time.deltaTime * 2f);
			____currentLookTarget = ___lookSpring.Update(____currentLookTarget, localPosition, Time.deltaTime);
			____animator.SetLookAtPosition(____animator.transform.TransformPoint(____currentLookTarget));
			____animator.SetLookAtWeight(____currentLookWeight);
			return false;

		}

		public static bool AnimController_OnZoneExit(CharacterAnimController __instance)
		{
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCharacterAnimController, CharacterAnimController>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBExitHeadZone, qsbObj.ObjectId);
			return false;
		}

		public static bool AnimController_OnZoneEntry(CharacterAnimController __instance)
		{
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCharacterAnimController, CharacterAnimController>(__instance);
			QSBEventManager.FireEvent(EventNames.QSBEnterHeadZone, qsbObj.ObjectId);
			return false;
		}
	}
}