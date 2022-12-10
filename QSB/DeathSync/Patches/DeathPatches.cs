using HarmonyLib;
using QSB.DeathSync.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

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
	[HarmonyPatch(typeof(DreamWorldController), nameof(DreamWorldController.ExitDreamWorld), typeof(DreamWakeType))]
	private static void ExitDreamWorld(DreamWorldController __instance, DreamWakeType wakeType = DreamWakeType.Default)
	{
		var deadPlayersCount = QSBPlayerManager.PlayerList.Count(x => x.IsDead);
		if ((deadPlayersCount != QSBPlayerManager.PlayerList.Count - 1 || QSBCore.DebugSettings.DisableLoopDeath) && PlayerState.IsResurrected())
		{
			__instance._wakeType = wakeType;
			__instance.CheckDreamZone2Completion();
			__instance.CheckSleepWakeDieAchievement(wakeType);

			__instance._activeGhostGrabController?.ReleasePlayer();
			__instance._activeZoomPoint?.CancelZoom();

			if (__instance._outsideLanternBounds)
			{
				__instance.EnterLanternBounds();
			}

			__instance._simulationCamera.OnExitDreamWorld();
			SunLightController.UnregisterSunOverrider(__instance);
			if (__instance._proxyShadowLight != null)
			{
				__instance._proxyShadowLight.enabled = true;
			}
			__instance._insideDream = false;
			__instance._waitingToLightLantern = false;
			__instance._playerLantern.OnExitDreamWorld();

			// TODO : drop player lantern at campfire

			Locator.GetPlayerSectorDetector().RemoveFromAllSectors();

			__instance._playerLantern.OnExitDreamWorld();
			__instance._dreamArrivalPoint.OnExitDreamWorld();
			__instance._dreamCampfire.OnDreamCampfireExtinguished -= __instance.OnDreamCampfireExtinguished;
			__instance._dreamCampfire = null;

			__instance.ExtinguishDreamRaft();
			Locator.GetAudioMixer().UnmixDreamWorld();
			Locator.GetAudioMixer().UnmixSleepAtCampfire(1f);

			if (__instance._playerCamAmbientLightRenderer != null)
			{
				__instance._playerCamAmbientLightRenderer.enabled = false;
			}

			__instance._playerCamera.cullingMask |= 1 << LayerMask.NameToLayer("Sun");
			__instance._playerCamera.farClipPlane = __instance._prevPlayerCameraFarPlaneDist;
			__instance._prevPlayerCameraFarPlaneDist = 0f;
			__instance._playerCamera.mainCamera.backgroundColor = Color.black;
			__instance._playerCamera.planetaryFog.enabled = true;
			__instance._playerCamera.postProcessingSettings.screenSpaceReflectionAvailable = false;
			__instance._playerCamera.postProcessingSettings.ambientOcclusionAvailable = true;

			GlobalMessenger.FireEvent("ExitDreamWorld");
		}
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
