using HarmonyLib;
using QSB.DeathSync.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using System.Linq;
using UnityEngine;

namespace QSB.DeathSync.Patches;

[HarmonyPatch]
public class DeathPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.OnImpact))]
	public static bool PlayerResources_OnImpact(PlayerResources __instance, ImpactData impact) =>
		// don't take damage from impact in ship
		!PlayerState.IsInsideShip();

	/// <summary>
	/// don't insta-die from impact in ship
	/// </summary>
	[HarmonyPrefix]
	[HarmonyPatch(typeof(HighSpeedImpactSensor), nameof(HighSpeedImpactSensor.HandlePlayerInsideShip))]
	public static bool HighSpeedImpactSensor_HandlePlayerInsideShip(HighSpeedImpactSensor __instance)
	{
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

			if (PlayerState.InDreamWorld() && deathType != DeathType.Dream && deathType != DeathType.DreamExplosion && deathType != DeathType.Supernova && deathType != DeathType.TimeLoop && deathType != DeathType.Meditation)
			{
				Locator.GetDreamWorldController().ExitDreamWorld(deathType);
				return;
			}

			if (!@this._isDying)
			{
				if (@this._invincible && deathType != DeathType.Supernova && deathType != DeathType.BigBang && deathType != DeathType.Meditation && deathType != DeathType.TimeLoop && deathType != DeathType.BlackHole)
				{
					return;
				}

				if (!Custom(@this, deathType))
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
						if (deathType == DeathType.TimeLoop || deathType == DeathType.BigBang || deathType == DeathType.Supernova)
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

		static bool Custom(DeathManager @this, DeathType deathType)
		{
			if (RespawnOnDeath.Instance == null)
			{
				return true;
			}

			if (RespawnOnDeath.Instance.AllowedDeathTypes.Contains(deathType))
			{
				return true;
			}

			if (@this.CheckShouldWakeInDreamWorld())
			{
				return true;
			}

			if (QSBPlayerManager.LocalPlayer.IsDead)
			{
				return false;
			}

			var deadPlayersCount = QSBPlayerManager.PlayerList.Count(x => x.IsDead);
			if (deadPlayersCount == QSBPlayerManager.PlayerList.Count - 1)
			{
				new EndLoopMessage().Send();
				return true;
			}

			RespawnOnDeath.Instance.ResetPlayer();

			QSBPlayerManager.LocalPlayer.IsDead = true;
			new PlayerDeathMessage(deathType).Send();

			if (PlayerAttachWatcher.Current)
			{
				PlayerAttachWatcher.Current.DetachPlayer();
			}

			return false;
		}
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
