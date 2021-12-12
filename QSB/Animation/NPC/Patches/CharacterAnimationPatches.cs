using HarmonyLib;
using OWML.Common;
using QSB.Animation.NPC.WorldObjects;
using QSB.ConversationSync;
using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Animation.NPC.Patches
{
	[HarmonyPatch]
	public class CharacterAnimationPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterAnimController), nameof(CharacterAnimController.OnAnimatorIK))]
		public static bool AnimatorIKReplacement(
			CharacterAnimController __instance,
			float ___headTrackingWeight,
			bool ___lookOnlyWhenTalking,
			bool ____inConversation,
			ref float ____currentLookWeight,
			ref Vector3 ____currentLookTarget,
			DampedSpring3D ___lookSpring,
			Animator ____animator,
			CharacterDialogueTree ____dialogueTree)
		{
			if (!WorldObjectManager.AllObjectsReady || ConversationManager.Instance == null)
			{
				return false;
			}

			var playerId = ConversationManager.Instance.GetPlayerTalkingToTree(____dialogueTree);
			var player = QSBPlayerManager.GetPlayer(playerId);
			var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCharacterAnimController>(__instance); // OPTIMIZE : maybe cache this somewhere... or assess how slow this is

			PlayerInfo playerToUse = null;
			if (____inConversation)
			{
				if (playerId == uint.MaxValue)
				{
					DebugLog.ToConsole($"Error - {__instance.name} is in conversation with a null player! Defaulting to active camera.", MessageType.Error);
					playerToUse = QSBPlayerManager.LocalPlayer;
				}
				else
				{
					playerToUse = player.CameraBody == null
						? QSBPlayerManager.LocalPlayer
						: player;
				}
			}
			else if (!___lookOnlyWhenTalking && qsbObj.GetPlayersInHeadZone().Count != 0) // IDEA : maybe this would be more fun if characters looked between players at random times? :P
			{
				playerToUse = QSBPlayerManager.GetClosestPlayerToWorldPoint(qsbObj.GetPlayersInHeadZone(), __instance.transform.position);
			}
			else if (QSBPlayerManager.PlayerList.Count != 0)
			{
				playerToUse = QSBPlayerManager.GetClosestPlayerToWorldPoint(__instance.transform.position, true);
			}

			var localPosition = playerToUse != null
				? ____animator.transform.InverseTransformPoint(playerToUse.CameraBody.transform.position)
				: Vector3.zero;

			var targetWeight = ___headTrackingWeight;
			if (___lookOnlyWhenTalking)
			{
				if (!____inConversation
					|| qsbObj.GetPlayersInHeadZone().Count == 0
					|| !qsbObj.GetPlayersInHeadZone().Contains(playerToUse))
				{
					targetWeight *= 0;
				}
			}
			else
			{
				if (qsbObj.GetPlayersInHeadZone().Count == 0
					|| !qsbObj.GetPlayersInHeadZone().Contains(playerToUse))
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

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterAnimController), nameof(CharacterAnimController.OnZoneExit))]
		public static bool HeadZoneExit(CharacterAnimController __instance, GameObject input)
		{
			if (input.CompareTag("PlayerDetector"))
			{
				var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCharacterAnimController>(__instance);
				QSBEventManager.FireEvent(EventNames.QSBExitNonNomaiHeadZone, qsbObj.ObjectId);
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterAnimController), nameof(CharacterAnimController.OnZoneEntry))]
		public static bool HeadZoneEntry(CharacterAnimController __instance, GameObject input)
		{
			if (input.CompareTag("PlayerDetector"))
			{
				var qsbObj = QSBWorldSync.GetWorldFromUnity<QSBCharacterAnimController>(__instance);
				QSBEventManager.FireEvent(EventNames.QSBEnterNonNomaiHeadZone, qsbObj.ObjectId);
			}
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(FacePlayerWhenTalking), nameof(FacePlayerWhenTalking.OnStartConversation))]
		public static bool OnStartConversation(FacePlayerWhenTalking __instance)
		{
			var playerId = ConversationManager.Instance.GetPlayerTalkingToTree(__instance._dialogueTree);
			if (playerId == uint.MaxValue)
			{
				DebugLog.ToConsole($"Error - No player talking to {__instance._dialogueTree.name}!", MessageType.Error);
				return false;
			}

			var player = QSBPlayerManager.GetPlayer(playerId);

			var distance = player.Body.transform.position - __instance.transform.position;
			var vector2 = distance - Vector3.Project(distance, __instance.transform.up);
			var angle = Vector3.Angle(__instance.transform.forward, vector2) * Mathf.Sign(Vector3.Dot(vector2, __instance.transform.right));
			var axis = __instance.transform.parent.InverseTransformDirection(__instance.transform.up);
			var lhs = Quaternion.AngleAxis(angle, axis);
			__instance.FaceLocalRotation(lhs * __instance.transform.localRotation);

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.StartConversation))]
		public static bool StartConversation(CharacterDialogueTree __instance)
		{
			var allNpcAnimControllers = QSBWorldSync.GetWorldObjects<INpcAnimController>();
			var ownerOfThis = allNpcAnimControllers.FirstOrDefault(x => x.GetDialogueTree() == __instance);
			if (ownerOfThis == default)
			{
				return true;
			}

			var id = ownerOfThis.ObjectId;
			QSBEventManager.FireEvent(EventNames.QSBNpcAnimEvent, AnimationEvent.StartConversation, id);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.EndConversation))]
		public static bool EndConversation(CharacterDialogueTree __instance)
		{
			var allNpcAnimControllers = QSBWorldSync.GetWorldObjects<INpcAnimController>();
			var ownerOfThis = allNpcAnimControllers.FirstOrDefault(x => x.GetDialogueTree() == __instance);
			if (ownerOfThis == default)
			{
				return true;
			}

			var id = ownerOfThis.ObjectId;
			QSBEventManager.FireEvent(EventNames.QSBNpcAnimEvent, AnimationEvent.EndConversation, id);
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(KidRockController), nameof(KidRockController.Update))]
		public static bool UpdateReplacement(KidRockController __instance)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return true;
			}

			var qsbObj = QSBWorldSync.GetWorldObjects<QSBCharacterAnimController>().First(x => x.GetDialogueTree() == __instance._dialogueTree);

			if (!__instance._throwingRock && !qsbObj.InConversation() && Time.time > __instance._nextThrowTime)
			{
				__instance.StartRockThrow();
			}

			return false;
		}
	}
}