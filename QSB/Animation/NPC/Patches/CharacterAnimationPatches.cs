using HarmonyLib;
using OWML.Common;
using QSB.Animation.NPC.WorldObjects;
using QSB.ConversationSync;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.TriggerSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Animation.NPC.Patches;

[HarmonyPatch]
public class CharacterAnimationPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CharacterAnimController), nameof(CharacterAnimController.OnAnimatorIK))]
	public static bool AnimatorIKReplacement(
		CharacterAnimController __instance)
	{
		if (!QSBWorldSync.AllObjectsReady || ConversationManager.Instance == null)
		{
			return true;
		}

		var playerId = ConversationManager.Instance.GetPlayerTalkingToTree(__instance._dialogueTree);
		var player = QSBPlayerManager.GetPlayer(playerId);

		if (__instance.playerTrackingZone == null)
		{
			return true;
		}

		var qsbObj = __instance.playerTrackingZone.GetWorldObject<QSBCharacterTrigger>();

		PlayerInfo playerToUse = null;
		if (__instance._inConversation)
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
		else if (!__instance.lookOnlyWhenTalking && qsbObj.Occupants.Count != 0) // IDEA : maybe this would be more fun if characters looked between players at random times? :P
		{
			playerToUse = QSBPlayerManager.GetClosestPlayerToWorldPoint(qsbObj.Occupants, __instance.transform.position);
		}
		else if (QSBPlayerManager.PlayerList.Count != 0)
		{
			playerToUse = QSBPlayerManager.GetClosestPlayerToWorldPoint(__instance.transform.position, true);
		}

		var localPosition = playerToUse != null
			? __instance._animator.transform.InverseTransformPoint(playerToUse.CameraBody.transform.position)
			: Vector3.zero;

		var targetWeight = __instance.headTrackingWeight;
		if (__instance.lookOnlyWhenTalking)
		{
			if (!__instance._inConversation
			    || qsbObj.Occupants.Count == 0
			    || !qsbObj.Occupants.Contains(playerToUse))
			{
				targetWeight *= 0;
			}
		}
		else
		{
			if (qsbObj.Occupants.Count == 0
			    || !qsbObj.Occupants.Contains(playerToUse))
			{
				targetWeight *= 0;
			}
		}

		__instance._currentLookWeight = Mathf.Lerp(__instance._currentLookWeight, targetWeight, Time.deltaTime * 2f);
		__instance._currentLookTarget = __instance.lookSpring.Update(__instance._currentLookTarget, localPosition, Time.deltaTime);
		__instance._animator.SetLookAtPosition(__instance._animator.transform.TransformPoint(__instance._currentLookTarget));
		__instance._animator.SetLookAtWeight(__instance._currentLookWeight);
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
}