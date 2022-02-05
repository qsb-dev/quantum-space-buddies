using HarmonyLib;
using QSB.DeathSync.Messages;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.ShipSync.TransformSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.DeathSync.Patches
{
	[HarmonyPatch]
	public class DeathPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		// TODO : Remove with future functionality.
		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipEjectionSystem), nameof(ShipEjectionSystem.OnPressInteract))]
		public static bool DisableEjection()
			=> false;

		// TODO : Remove with future functionality.
		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipDetachableLeg), nameof(ShipDetachableLeg.Detach))]
		public static bool ShipDetachableLeg_Detach(ref OWRigidbody __result)
		{
			__result = null;
			return false;
		}

		// TODO : Remove with future functionality.
		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipDetachableModule), nameof(ShipDetachableModule.Detach))]
		public static bool ShipDetachableModule_Detach(ref OWRigidbody __result)
		{
			__result = null;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PlayerResources), nameof(PlayerResources.OnImpact))]
		public static bool PlayerResources_OnImpact(PlayerResources __instance, ImpactData impact)
		{
			if (PlayerState.IsInsideShip())
			{
				return false;
			}

			var speed = Mathf.Clamp01((impact.speed - __instance.GetMinImpactSpeed()) / (__instance.GetMaxImpactSpeed() - __instance.GetMinImpactSpeed()));
			var tookDamage = __instance.ApplyInstantDamage(100f * speed, InstantDamageType.Impact);
			if (tookDamage && __instance._currentHealth <= 0f && !PlayerState.IsDead())
			{
				Locator.GetDeathManager().SetImpactDeathSpeed(impact.speed);
				Locator.GetDeathManager().KillPlayer(DeathType.Impact);
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HighSpeedImpactSensor), nameof(HighSpeedImpactSensor.FixedUpdate))]
		public static bool HighSpeedImpactSensor_FixedUpdate(
			HighSpeedImpactSensor __instance
			)
		{
			if (__instance._isPlayer && (PlayerState.IsAttached() || PlayerState.IsInsideShuttle() || PlayerState.UsingNomaiRemoteCamera()))
			{
				return false;
			}

			if (__instance._dieNextUpdate && !__instance._dead)
			{
				__instance._dead = true;
				__instance._dieNextUpdate = false;
				if (__instance.gameObject.CompareTag("Player"))
				{
					Locator.GetDeathManager().SetImpactDeathSpeed(__instance._impactSpeed);
					Locator.GetDeathManager().KillPlayer(DeathType.Impact);
				}
				else if (__instance.gameObject.CompareTag("Ship"))
				{
					__instance.GetComponent<ShipDamageController>().Explode();
				}
			}

			if (__instance._isPlayer && PlayerState.IsInsideShip())
			{
				var shipCenter = Locator.GetShipTransform().position + (Locator.GetShipTransform().up * 2f);
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

			var passiveReferenceFrame = __instance._sectorDetector.GetPassiveReferenceFrame();
			if (!__instance._dead && passiveReferenceFrame != null)
			{
				var relativeVelocity = __instance._body.GetVelocity() - passiveReferenceFrame.GetOWRigidBody().GetPointVelocity(__instance._body.GetPosition());
				if (relativeVelocity.sqrMagnitude > __instance._sqrCheckSpeedThreshold)
				{
					var hitCount = Physics.RaycastNonAlloc(__instance.transform.TransformPoint(__instance._localOffset), relativeVelocity, __instance._raycastHits, (relativeVelocity.magnitude * Time.deltaTime) + __instance._radius, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
					for (var i = 0; i < hitCount; i++)
					{
						if (__instance._raycastHits[i].rigidbody.mass > 10f && !__instance._raycastHits[i].rigidbody.Equals(__instance._body.GetRigidbody()))
						{
							var owRigidbody = __instance._raycastHits[i].rigidbody.GetComponent<OWRigidbody>();
							if (owRigidbody == null)
							{
								DebugLog.ToConsole("Rigidbody does not have attached OWRigidbody!!!", OWML.Common.MessageType.Error);
								Debug.Break();
							}
							else
							{
								relativeVelocity = __instance._body.GetVelocity() - owRigidbody.GetPointVelocity(__instance._body.GetPosition());
								var a2 = Vector3.Project(relativeVelocity, __instance._raycastHits[i].normal);
								if (a2.sqrMagnitude > __instance._sqrCheckSpeedThreshold)
								{
									__instance._body.AddVelocityChange(-a2);
									__instance._impactSpeed = a2.magnitude;
									if (!PlayerState.IsInsideTheEye())
									{
										__instance._dieNextUpdate = true;
									}

									break;
								}
							}
						}
					}
				}
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(DeathManager), nameof(DeathManager.KillPlayer))]
		public static bool DeathManager_KillPlayer_Prefix(DeathType deathType)
		{
			if (RespawnOnDeath.Instance == null)
			{
				return true;
			}

			if (RespawnOnDeath.Instance.AllowedDeathTypes.Contains(deathType))
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
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(DeathManager), nameof(DeathManager.KillPlayer))]
		public static void DeathManager_KillPlayer_Postfix(DeathType deathType)
		{
			if (QSBPlayerManager.LocalPlayer.IsDead)
			{
				return;
			}

			QSBPlayerManager.LocalPlayer.IsDead = true;
			new PlayerDeathMessage(deathType).Send();

			if (PlayerAttachWatcher.Current)
			{
				PlayerAttachWatcher.Current.DetachPlayer();
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ShipDamageController), nameof(ShipDamageController.Awake))]
		public static void ShipDamageController_Awake(ShipDamageController __instance)
			=> __instance._exploded = true;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(DestructionVolume), nameof(DestructionVolume.VanishShip))]
		public static bool DestructionVolume_VanishShip(DestructionVolume __instance)
		{
			if (RespawnOnDeath.Instance == null)
			{
				return true;
			}

			if (!ShipTransformSync.LocalInstance.hasAuthority)
			{
				return false;
			}

			if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || PlayerState.AtFlightConsole())
			{
				Locator.GetDeathManager().KillPlayer(__instance._deathType);
			}

			return true;
		}
	}
}
