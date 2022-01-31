using HarmonyLib;
using QSB.Animation.Player.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.Animation.Player.Patches
{
	[HarmonyPatch]
	internal class PlayerAnimationPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.LateUpdate))]
		public static bool LateUpdateReplacement(
				PlayerAnimController __instance)
		{
			var isGrounded = __instance._playerController.IsGrounded();
			var isAttached = PlayerState.IsAttached();
			var isInZeroG = PlayerState.InZeroG();
			var isFlying = __instance._playerJetpack.GetLocalAcceleration().y > 0f;
			var movementVector = Vector3.zero;
			if (!isAttached)
			{
				movementVector = __instance._playerController.GetRelativeGroundVelocity();
			}

			if (Mathf.Abs(movementVector.x) < 0.05f)
			{
				movementVector.x = 0f;
			}

			if (Mathf.Abs(movementVector.z) < 0.05f)
			{
				movementVector.z = 0f;
			}

			if (isFlying)
			{
				__instance._ungroundedTime = Time.time;
			}

			var freefallMagnitude = 0f;
			var timeInFreefall = 0f;
			var lastGroundBody = __instance._playerController.GetLastGroundBody();
			if (!isGrounded && !isAttached && !isInZeroG && lastGroundBody != null)
			{
				freefallMagnitude = (__instance._playerController.GetAttachedOWRigidbody().GetVelocity() - lastGroundBody.GetPointVelocity(__instance._playerController.transform.position)).magnitude;
				timeInFreefall = Time.time - __instance._ungroundedTime;
			}

			__instance._animator.SetFloat("RunSpeedX", movementVector.x / 3f);
			__instance._animator.SetFloat("RunSpeedY", movementVector.z / 3f);
			__instance._animator.SetFloat("TurnSpeed", __instance._playerController.GetTurning());
			__instance._animator.SetBool("Grounded", isGrounded || isAttached || PlayerState.IsRecentlyDetached());
			__instance._animator.SetLayerWeight(1, __instance._playerController.GetJumpCrouchFraction());
			__instance._animator.SetFloat("FreefallSpeed", freefallMagnitude / 15f * (timeInFreefall / 3f));
			__instance._animator.SetBool("InZeroG", isInZeroG || isFlying);
			__instance._animator.SetBool("UsingJetpack", isInZeroG && PlayerState.IsWearingSuit());
			if (__instance._justBecameGrounded)
			{
				if (__instance._justTookFallDamage)
				{
					__instance._animator.SetTrigger("LandHard");
					new AnimationTriggerMessage("LandHard").Send();
				}
				else
				{
					__instance._animator.SetTrigger("Land");
					new AnimationTriggerMessage("Land").Send();
				}
			}

			if (isGrounded)
			{
				var leftFootLift = __instance._animator.GetFloat("LeftFootLift");
				if (!__instance._leftFootGrounded && leftFootLift < 0.333f)
				{
					__instance._leftFootGrounded = true;
					__instance.RaiseEvent(nameof(__instance.OnLeftFootGrounded));
				}
				else if (__instance._leftFootGrounded && leftFootLift > 0.666f)
				{
					__instance._leftFootGrounded = false;
					__instance.RaiseEvent(nameof(__instance.OnLeftFootLift));
				}

				var rightFootLift = __instance._animator.GetFloat("RightFootLift");
				if (!__instance._rightFootGrounded && rightFootLift < 0.333f)
				{
					__instance._rightFootGrounded = true;
					__instance.RaiseEvent(nameof(__instance.OnRightFootGrounded));
				}
				else if (__instance._rightFootGrounded && rightFootLift > 0.666f)
				{
					__instance._rightFootGrounded = false;
					__instance.RaiseEvent(nameof(__instance.OnRightFootLift));
				}
			}

			__instance._justBecameGrounded = false;
			__instance._justTookFallDamage = false;
			var usingTool = Locator.GetToolModeSwapper().GetToolMode() != ToolMode.None;
			if ((usingTool && !__instance._rightArmHidden) || (!usingTool && __instance._rightArmHidden))
			{
				__instance._rightArmHidden = usingTool;
				for (var i = 0; i < __instance._rightArmObjects.Length; i++)
				{
					__instance._rightArmObjects[i].layer = (!__instance._rightArmHidden) ? __instance._defaultLayer : __instance._probeOnlyLayer;
				}
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerAnimController), nameof(PlayerAnimController.OnPlayerJump))]
		public static bool OnPlayerJumpReplacement(PlayerAnimController __instance)
		{
			__instance._ungroundedTime = Time.time;
			if (!__instance.isActiveAndEnabled)
			{
				return false;
			}

			__instance._animator.SetTrigger("Jump");
			new AnimationTriggerMessage("Jump").Send();
			return false;
		}
	}
}
