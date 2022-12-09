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
		if (deadPlayersCount != QSBPlayerManager.PlayerList.Count - 1 && PlayerState.IsResurrected())
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
	[HarmonyPatch(typeof(DeathManager), nameof(DeathManager.KillPlayer))]
	private static bool DeathManager_KillPlayer(DeathManager __instance, DeathType deathType)
	{
		// funny moment for eye
		if (QSBSceneManager.CurrentScene != OWScene.SolarSystem)
		{
			return true;
		}

		Original(__instance, deathType);
		return false;

		static void Original(DeathManager @this, DeathType deathType)
		{
			@this._fakeMeditationDeath = false;
			if (deathType == DeathType.Meditation && @this.CheckShouldWakeInDreamWorld())
			{
				@this._fakeMeditationDeath = true;
				OWInput.ChangeInputMode(InputMode.None);
				ReticleController.Hide();
				Locator.GetPromptManager().SetPromptsVisible(false);
				GlobalMessenger.FireEvent("FakePlayerMeditationDeath");
				return;
			}

			if (deathType == DeathType.DreamExplosion)
			{
				Achievements.Earn(Achievements.Type.EARLY_ADOPTER);
			}

			if (PlayerState.InDreamWorld()
				&& deathType != DeathType.Dream
				&& deathType != DeathType.DreamExplosion
				&& deathType != DeathType.Supernova
				&& deathType != DeathType.TimeLoop
				&& deathType != DeathType.Meditation)
			{
				Locator.GetDreamWorldController().ExitDreamWorld(deathType);
				return;
			}

			if (!@this._isDying)
			{
				if (@this._invincible
					&& deathType != DeathType.Supernova
					&& deathType != DeathType.BigBang
					&& deathType != DeathType.Meditation
					&& deathType != DeathType.TimeLoop
					&& deathType != DeathType.BlackHole)
				{
					return;
				}

				if (!TimeLoopCoreController.ParadoxExists())
				{
					var component = Locator.GetPlayerBody().GetComponent<PlayerResources>();
					if ((deathType == DeathType.TimeLoop || deathType == DeathType.Supernova) && component.GetTotalDamageThisLoop() > 1000f)
					{
						Achievements.Earn(Achievements.Type.DIEHARD);
						PlayerData.SetPersistentCondition("THERE_IS_BUT_VOID", true);
					}

					if (((TimeLoop.GetLoopCount() != 1 && TimeLoop.GetSecondsElapsed() < 60f) || (TimeLoop.GetLoopCount() == 1 && Time.timeSinceLevelLoad < 60f && !TimeLoop.IsTimeFlowing())) && deathType != DeathType.Meditation && LoadManager.GetCurrentScene() == OWScene.SolarSystem)
					{
						Achievements.Earn(Achievements.Type.GONE_IN_60_SECONDS);
					}

					if (TimeLoop.GetLoopCount() > 1)
					{
						Achievements.SetHeroStat(Achievements.HeroStat.TIMELOOP_COUNT, (uint)(TimeLoop.GetLoopCount() - 1));
						if (deathType is DeathType.TimeLoop or DeathType.BigBang or DeathType.Supernova)
						{
							PlayerData.CompletedFullTimeLoop();
						}
					}

					if (deathType == DeathType.Supernova && !PlayerData.GetPersistentCondition("KILLED_BY_SUPERNOVA_AND_KNOWS_IT") && PlayerData.GetFullTimeLoopsCompleted() > 2U && PlayerData.GetPersistentCondition("HAS_SEEN_SUN_EXPLODE"))
					{
						PlayerData.SetPersistentCondition("KILLED_BY_SUPERNOVA_AND_KNOWS_IT", true);
						MonoBehaviour.print("KILLED_BY_SUPERNOVA_AND_KNOWS_IT");
					}
				}

				@this._isDying = true;
				@this._deathType = deathType;
				MonoBehaviour.print("Player was killed by " + deathType);
				Locator.GetPauseCommandListener().AddPauseCommandLock();
				PlayerData.SetLastDeathType(deathType);
				GlobalMessenger<DeathType>.FireEvent("PlayerDeath", deathType);
			}
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
			if (deadPlayersCount == QSBPlayerManager.PlayerList.Count - 1)
			{
				new EndLoopMessage().Send();
				QSBPlayerManager.LocalPlayer.IsDead = true;
				DebugLog.DebugWrite($"- All players are dead.");
				return false;
			}

			if (!RespawnOnDeath.Instance.AllowedDeathTypes.Contains(__instance._deathType))
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
