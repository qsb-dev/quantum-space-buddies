using HarmonyLib;
using QSB.Events;
using QSB.Patches;
using QSB.Player;
using QSB.ShipSync;
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
		public static bool PlayerResources_OnImpact(ImpactData impact, PlayerResources __instance, float ____currentHealth)
		{
			if (PlayerState.IsInsideShip())
			{
				return false;
			}

			var speed = Mathf.Clamp01((impact.speed - __instance.GetMinImpactSpeed()) / (__instance.GetMaxImpactSpeed() - __instance.GetMinImpactSpeed()));
			var tookDamage = __instance.ApplyInstantDamage(100f * speed, InstantDamageType.Impact);
			if (tookDamage && ____currentHealth <= 0f && !PlayerState.IsDead())
			{
				Locator.GetDeathManager().SetImpactDeathSpeed(impact.speed);
				Locator.GetDeathManager().KillPlayer(DeathType.Impact);
				DebugLog.DebugWrite(string.Concat(new object[]
				{
				"Player killed from impact with ",
				impact.otherCollider,
				" attached to ",
				impact.otherCollider.attachedRigidbody.gameObject.name
				}));
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HighSpeedImpactSensor), nameof(HighSpeedImpactSensor.FixedUpdate))]
		public static bool HighSpeedImpactSensor_FixedUpdate(
			HighSpeedImpactSensor __instance,
			bool ____isPlayer,
			ref bool ____dead,
			ref bool ____dieNextUpdate,
			OWRigidbody ____body,
			ref float ____impactSpeed,
			float ____sqrCheckSpeedThreshold,
			RaycastHit[] ____raycastHits,
			SectorDetector ____sectorDetector,
			float ____radius,
			Vector3 ____localOffset
			)
		{
			if (____isPlayer && (PlayerState.IsAttached() || PlayerState.IsInsideShuttle() || PlayerState.UsingNomaiRemoteCamera()))
			{
				return false;
			}

			if (____dieNextUpdate && !____dead)
			{
				____dead = true;
				____dieNextUpdate = false;
				if (__instance.gameObject.CompareTag("Player"))
				{
					Locator.GetDeathManager().SetImpactDeathSpeed(____impactSpeed);
					Locator.GetDeathManager().KillPlayer(DeathType.Impact);
				}
				else if (__instance.gameObject.CompareTag("Ship"))
				{
					__instance.GetComponent<ShipDamageController>().Explode(false);
				}
			}

			if (____isPlayer && PlayerState.IsInsideShip())
			{
				var shipCenter = Locator.GetShipTransform().position + (Locator.GetShipTransform().up * 2f);
				var distanceFromShip = Vector3.Distance(____body.GetPosition(), shipCenter);
				if (distanceFromShip > 8f)
				{
					____body.SetPosition(shipCenter);
					DebugLog.DebugWrite("MOVE PLAYER BACK TO SHIP CENTER");
				}

				if (!____dead)
				{
					var a = ____body.GetVelocity() - Locator.GetShipBody().GetPointVelocity(____body.GetPosition());
					if (a.sqrMagnitude > ____sqrCheckSpeedThreshold)
					{
						____impactSpeed = a.magnitude;
						____body.AddVelocityChange(-a);
						DebugLog.DebugWrite("Would have killed player...");
						//____dieNextUpdate = true;
						DebugLog.DebugWrite(string.Concat(new object[]
						{
						"HIGH SPEED IMPACT: ",
						__instance.name,
						" hit the Ship at ",
						____impactSpeed,
						"m/s   Dist from ship: ",
						distanceFromShip
						}));
					}
				}

				return false;
			}

			var passiveReferenceFrame = ____sectorDetector.GetPassiveReferenceFrame();
			if (!____dead && passiveReferenceFrame != null)
			{
				var relativeVelocity = ____body.GetVelocity() - passiveReferenceFrame.GetOWRigidBody().GetPointVelocity(____body.GetPosition());
				if (relativeVelocity.sqrMagnitude > ____sqrCheckSpeedThreshold)
				{
					var hitCount = Physics.RaycastNonAlloc(__instance.transform.TransformPoint(____localOffset), relativeVelocity, ____raycastHits, (relativeVelocity.magnitude * Time.deltaTime) + ____radius, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
					for (var i = 0; i < hitCount; i++)
					{
						if (____raycastHits[i].rigidbody.mass > 10f && !____raycastHits[i].rigidbody.Equals(____body.GetRigidbody()))
						{
							var owRigidbody = ____raycastHits[i].rigidbody.GetComponent<OWRigidbody>();
							if (owRigidbody == null)
							{
								DebugLog.ToConsole("Rigidbody does not have attached OWRigidbody!!!", OWML.Common.MessageType.Error);
								Debug.Break();
							}
							else
							{
								relativeVelocity = ____body.GetVelocity() - owRigidbody.GetPointVelocity(____body.GetPosition());
								var a2 = Vector3.Project(relativeVelocity, ____raycastHits[i].normal);
								if (a2.sqrMagnitude > ____sqrCheckSpeedThreshold)
								{
									____body.AddVelocityChange(-a2);
									____impactSpeed = a2.magnitude;
									if (!PlayerState.IsInsideTheEye())
									{
										____dieNextUpdate = true;
									}

									DebugLog.DebugWrite(string.Concat(new object[]
									{
									"HIGH SPEED IMPACT: ",
									__instance.name,
									" hit ",
									____raycastHits[i].rigidbody.name,
									" at ",
									____impactSpeed,
									"m/s   RF: ",
									passiveReferenceFrame.GetOWRigidBody().name
									}));
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

			QSBPlayerManager.LocalPlayer.IsDead = true;

			RespawnOnDeath.Instance.ResetPlayer();
			return false;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(DeathManager), nameof(DeathManager.KillPlayer))]
		public static void DeathManager_KillPlayer_Postfix(DeathType deathType) => QSBEventManager.FireEvent(EventNames.QSBPlayerDeath, deathType);

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ShipDamageController), nameof(ShipDamageController.Awake))]
		public static void ShipDamageController_Awake(ref bool ____exploded)
			=> ____exploded = true;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(DestructionVolume), nameof(DestructionVolume.VanishShip))]
		public static bool DestructionVolume_VanishShip(DeathType ____deathType)
		{
			if (RespawnOnDeath.Instance == null)
			{
				return true;
			}

			if (!ShipManager.Instance.HasAuthority)
			{
				return false;
			}

			if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || PlayerState.AtFlightConsole())
			{
				Locator.GetDeathManager().KillPlayer(____deathType);
			}

			return true;
		}
	}
}