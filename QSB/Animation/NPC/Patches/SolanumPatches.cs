using HarmonyLib;
using QSB.Animation.NPC.WorldObjects;
using QSB.Patches;
using QSB.Player;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Animation.NPC.Patches;

[HarmonyPatch]
public class SolanumPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(SolanumAnimController), nameof(SolanumAnimController.LateUpdate))]
	public static bool SolanumLateUpdateReplacement(SolanumAnimController __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		if (__instance._animatorStateEvents == null)
		{
			__instance._animatorStateEvents = __instance._animator.GetBehaviour<AnimatorStateEvents>();
			__instance._animatorStateEvents.OnEnterState += __instance.OnEnterAnimatorState;
		}

		var qsbObj = __instance.GetWorldObject<QSBSolanumAnimController>();
		var playersInHeadZone = qsbObj.Trigger.Occupants;

		var targetCamera = playersInHeadZone == null || playersInHeadZone.Count == 0
			? __instance._playerCameraTransform
			: QSBPlayerManager.GetClosestPlayerToWorldPoint(playersInHeadZone, __instance.transform.position).CameraBody.transform;

		var targetValue = Quaternion.LookRotation(targetCamera.position - __instance._headBoneTransform.position, __instance.transform.up);
		__instance._currentLookRotation = __instance._lookSpring.Update(__instance._currentLookRotation, targetValue, Time.deltaTime);

		var position = __instance._headBoneTransform.position + (__instance._currentLookRotation * Vector3.forward);
		__instance._localLookPosition = __instance.transform.InverseTransformPoint(position);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(NomaiConversationManager), nameof(NomaiConversationManager.Update))]
	public static bool ReplacementUpdate(NomaiConversationManager __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}
		var qsbObj = __instance._solanumAnimController.GetWorldObject<QSBSolanumAnimController>();
		__instance._playerInWatchVolume = qsbObj.Trigger.Occupants.Any();

		if (!__instance._initialized)
		{
			__instance.InitializeNomaiText();
		}

		//var heldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
		//var holdingConversationStone = heldItem != null && heldItem is NomaiConversationStone;
		var holdingConversationStone = QSBPlayerManager.GetPlayerCarryItems().Any(x => x.HeldItem != null && x.HeldItem.GetItemType() == ItemType.ConversationStone);

		switch (__instance._state)
		{
			case NomaiConversationManager.State.WatchingSky:
				if (__instance._playerInWatchVolume)
				{
					__instance._state = NomaiConversationManager.State.WatchingPlayer;
					__instance._solanumAnimController.StartWatchingPlayer();
				}

				break;

			case NomaiConversationManager.State.WatchingPlayer:
				if (!__instance._solanumAnimController.isPerformingAction)
				{
					// player left watch zone
					if (!__instance._playerInWatchVolume)
					{
						__instance._state = NomaiConversationManager.State.WatchingSky;
						__instance._solanumAnimController.StopWatchingPlayer();
					}
					else if (__instance._dialogueComplete)
					{
						// create conversation stones
						if (__instance._dialogueComplete && !__instance._conversationStonesCreated)
						{
							__instance._stoneCreationTimer -= Time.deltaTime;
							if (__instance._stoneCreationTimer <= 0f)
							{
								__instance._state = NomaiConversationManager.State.CreatingStones;
								__instance._solanumAnimController.PlayCreateWordStones();
							}
						}
						else if (__instance._conversationStonesCreated && !__instance._cairnRaised)
						{
							if (!holdingConversationStone)
							{
								__instance._stoneGestureTimer -= Time.deltaTime;
								if (__instance._stoneGestureTimer <= 0f)
								{
									__instance._solanumAnimController.PlayGestureToWordStones();
									__instance._stoneGestureTimer = UnityEngine.Random.Range(8f, 16f);
								}
							}
							// raise cairns
							else if (__instance._solanumAnimController.IsPlayerLooking())
							{
								__instance._state = NomaiConversationManager.State.RaisingCairns;
								__instance._solanumAnimController.PlayRaiseCairns();
								__instance._cairnAnimator.SetTrigger("Raise");
								__instance._cairnCollision.SetActivation(true);
							}
						}
						else if (__instance._activeResponseText == null && __instance._hasValidSocketedStonePair)
						{
							__instance._activeResponseText = __instance._pendingResponseText;
							__instance._pendingResponseText = null;
							__instance._state = NomaiConversationManager.State.WritingResponse;
							__instance._solanumAnimController.StartWritingMessage();
						}
						else if (__instance._activeResponseText != null && (!__instance._hasValidSocketedStonePair || __instance._pendingResponseText != null))
						{
							__instance._state = NomaiConversationManager.State.ErasingResponse;
							__instance._solanumAnimController.StartWritingMessage();
						}
						else if (!holdingConversationStone)
						{
							if (__instance._playerWasHoldingStone)
							{
								__instance.ResetStoneGestureTimer();
							}

							__instance._stoneGestureTimer -= Time.deltaTime;
							if (__instance._stoneGestureTimer < 0f)
							{
								__instance._solanumAnimController.PlayGestureToWordStones();
								__instance.ResetStoneGestureTimer();
							}
						}
						else
						{
							if (!__instance._playerWasHoldingStone)
							{
								__instance.ResetCairnGestureTimer();
							}

							__instance._cairnGestureTimer -= Time.deltaTime;
							if (__instance._cairnGestureTimer < 0f)
							{
								__instance._solanumAnimController.PlayGestureToCairns();
								__instance.ResetCairnGestureTimer();
							}
						}
					}
				}

				break;

			case NomaiConversationManager.State.CreatingStones:
				if (!__instance._solanumAnimController.isPerformingAction)
				{
					__instance._state = NomaiConversationManager.State.WatchingPlayer;
					__instance._conversationStonesCreated = true;
				}

				break;

			case NomaiConversationManager.State.RaisingCairns:
				if (!__instance._solanumAnimController.isPerformingAction)
				{
					__instance._state = NomaiConversationManager.State.WatchingPlayer;
					__instance._cairnRaised = true;
					__instance._stoneSocketATrigger.SetActivation(true);
					__instance._stoneSocketBTrigger.SetActivation(true);
				}

				break;

			case NomaiConversationManager.State.ErasingResponse:
				if (!__instance._solanumAnimController.isStartingWrite && !__instance._activeResponseText.IsAnimationPlaying())
				{
					__instance._activeResponseText = null;
					if (__instance._pendingResponseText == null)
					{
						__instance._state = NomaiConversationManager.State.WatchingPlayer;
						__instance._solanumAnimController.StopWritingMessage(false);
					}
					else
					{
						__instance._activeResponseText = __instance._pendingResponseText;
						__instance._pendingResponseText = null;
						__instance._state = NomaiConversationManager.State.WritingResponse;
						__instance._activeResponseText.Show();
					}
				}

				break;

			case NomaiConversationManager.State.WritingResponse:
				if (!__instance._solanumAnimController.isStartingWrite && !__instance._activeResponseText.IsAnimationPlaying())
				{
					__instance._state = NomaiConversationManager.State.WatchingPlayer;
					__instance._solanumAnimController.StopWritingMessage(true);
				}

				break;
		}

		__instance._playerWasHoldingStone = holdingConversationStone;

		return false;
	}
}
