using HarmonyLib;
using QSB.DeathSync.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.DeathSync.Patches;

[HarmonyPatch]
public class DeathPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	/// <summary>
	/// don't take damage from impact in ship
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.OnImpact))]
	public static bool PlayerResources_OnImpact(PlayerResources __instance, ImpactData impact)
	{
		if (QSBCore.ShipDamage)
		{
			return true;
		}

		return !PlayerState.IsInsideShip();
	}

	/// <summary>
	/// don't insta-die from impact in ship
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(HighSpeedImpactSensor), nameof(HighSpeedImpactSensor.HandlePlayerInsideShip))]
	public static bool HighSpeedImpactSensor_HandlePlayerInsideShip(HighSpeedImpactSensor __instance)
	{
		if (QSBCore.ShipDamage)
		{
			return true;
		}

		var shipCenter = Locator.GetShipTransform().position + Locator.GetShipTransform().up * 2f;
		var distanceFromShip = Vector3.Distance(__instance._body.GetPosition(), shipCenter);
		if (distanceFromShip > 8f)
		{
			__instance._body.SetPosition(shipCenter);
		}

		if (!__instance._dead)
		{
			var a = __instance._body.GetVelocity() - Locator.GetShipBody().GetPointVelocity(__instance._body.GetPosition());
			if (a.sqrMagnitude > __instance._sqrCheckSpeedThreshold)
			{
				__instance._impactSpeed = a.magnitude;
				__instance._body.AddVelocityChange(-a);
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DeathManager), nameof(DeathManager.FinishDeathSequence))]
	public static bool FinishDeathSequence(DeathManager __instance)
	{
		if (!__instance._isDead)
		{
			if (__instance.CheckShouldWakeInDreamWorld())
			{
				__instance.enabled = true;
				__instance._resurrectAfterDelay = true;
				__instance._resurrectTime = Time.time + 2f;
				return false;
			}

			var deadPlayersCount = QSBPlayerManager.PlayerList.Count(x => x.IsDead);
			if (deadPlayersCount == QSBPlayerManager.PlayerList.Count - 1 && !QSBCore.DebugSettings.DisableLoopDeath)
			{
				new EndLoopMessage().Send();
				DebugLog.DebugWrite($"- All players are dead.");
			}
			else if (!RespawnOnDeath.Instance.AllowedDeathTypes.Contains(__instance._deathType))
			{
				RespawnOnDeath.Instance.ResetPlayer();
				QSBPlayerManager.LocalPlayer.IsDead = true;
				new PlayerDeathMessage(__instance._deathType).Send();
				if (PlayerAttachWatcher.Current)
				{
					PlayerAttachWatcher.Current.DetachPlayer();
				}

				return false;
			}

			__instance._isDead = true;
			GlobalMessenger.FireEvent("DeathSequenceComplete");
			if (PlayerData.GetPersistentCondition("DESTROYED_TIMELINE_LAST_SAVE"))
			{
				PlayerData.SetPersistentCondition("DESTROYED_TIMELINE_LAST_SAVE", false);
			}

			if (__instance._deathType == DeathType.BigBang)
			{
				if (TimeLoopCoreController.ParadoxExists())
				{
					PlayerData.RevertParadoxLoopCountStates();
				}

				LoadManager.LoadScene(OWScene.Credits_Final, LoadManager.FadeType.ToWhite, 1f, true);
				return false;
			}

			if (TimeLoopCoreController.ParadoxExists())
			{
				Locator.GetTimelineObliterationController().BeginTimelineObliteration(TimelineObliterationController.ObliterationType.PARADOX_DEATH, null);
				return false;
			}

			if (TimeLoop.IsTimeFlowing() && TimeLoop.IsTimeLoopEnabled())
			{
				if (__instance._finishedDLC)
				{
					GlobalMessenger.FireEvent("TriggerEndOfDLC");
					return false;
				}
				GlobalMessenger.FireEvent("TriggerFlashback");
				return false;
			}
			else
			{
				if (__instance._deathType == DeathType.Meditation && PlayerState.OnQuantumMoon() && Locator.GetQuantumMoon().GetStateIndex() == 5)
				{
					__instance._timeloopEscapeType = TimeloopEscapeType.Quantum;
					__instance.FinishEscapeTimeLoopSequence();
					return false;
				}
				GlobalMessenger.FireEvent("TriggerDeathOutsideTimeLoop");
			}
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PauseMenuManager), nameof(PauseMenuManager.Update))]
	public static bool PauseMenuManager_Update(PauseMenuManager __instance)
	{
		// disable meditate button when in respawn map

		if (__instance._waitingToApplySkipLoopStyle)
		{
			var disableMeditate = PlayerState.IsSleepingAtCampfire() || PlayerState.IsGrabbedByGhost() || QSBPlayerManager.LocalPlayer.IsDead;
			__instance._skipToNextLoopButton.GetComponent<UIStyleApplier>().ChangeState(disableMeditate ? UIElementState.DISABLED : UIElementState.NORMAL, false);
			__instance._waitingToApplySkipLoopStyle = false;
		}

		if (OWInput.IsNewlyPressed(InputLibrary.pause, InputMode.All) && __instance._isOpen && MenuStackManager.SharedInstance.GetMenuCount() == 1)
		{
			__instance._pauseMenu.EnableMenu(false);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PauseMenuManager), nameof(PauseMenuManager.OnActivateMenu))]
	public static bool PauseMenuManager_OnActivateMenu(PauseMenuManager __instance)
	{
		if (__instance._skipToNextLoopButton.activeSelf)
		{
			bool flag = !PlayerState.IsSleepingAtCampfire() && !PlayerState.IsGrabbedByGhost() && !QSBPlayerManager.LocalPlayer.IsDead;
			__instance._endCurrentLoopAction.enabled = flag;
			__instance._skipToNextLoopButton.GetComponent<Selectable>().interactable = flag;
			__instance._skipToNextLoopButton.GetComponent<UIStyleApplier>().SetAutoInputStateChangesEnabled(flag);
			__instance._waitingToApplySkipLoopStyle = true;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(DestructionVolume), nameof(DestructionVolume.VanishShip))]
	public static bool DestructionVolume_VanishShip(DestructionVolume __instance, OWRigidbody shipBody, RelativeLocationData entryLocation)
	{
		var cockpitIntact = !shipBody.GetComponent<ShipDamageController>().IsCockpitDetached();
		if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || (cockpitIntact && PlayerState.AtFlightConsole()))
		{
			var autopilot = shipBody.GetComponent<Autopilot>();
			if (autopilot != null && autopilot.IsFlyingToDestination())
			{
				var astroObject = __instance.GetComponentInParent<AstroObject>();
				if (astroObject != null && astroObject.GetAstroObjectType() == AstroObject.Type.Star)
				{
					PlayerData.SetPersistentCondition("AUTOPILOT_INTO_SUN", true);
					MonoBehaviour.print("AUTOPILOT_INTO_SUN");
				}
			}

			Locator.GetDeathManager().KillPlayer(__instance._deathType);
		}

		__instance.Vanish(shipBody, entryLocation);
		GlobalMessenger.FireEvent("ShipDestroyed");
		return false;
	}
}
