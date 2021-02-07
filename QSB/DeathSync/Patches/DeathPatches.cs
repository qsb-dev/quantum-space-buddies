using Harmony;
using QSB.Events;
using QSB.Patches;
using QSB.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace QSB.DeathSync.Patches
{
	public class DeathPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnModStart;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<DeathManager>("KillPlayer", typeof(DeathPatches), nameof(PreFinishDeathSequence));
			QSBCore.Helper.HarmonyHelper.AddPostfix<DeathManager>("KillPlayer", typeof(DeathPatches), nameof(BroadcastDeath));
			QSBCore.Helper.HarmonyHelper.Transpile<ShipDetachableLeg>("Detach", typeof(DeathPatches), nameof(ReturnNull));
			QSBCore.Helper.HarmonyHelper.Transpile<ShipDetachableModule>("Detach", typeof(DeathPatches), nameof(ReturnNull));
			QSBCore.Helper.HarmonyHelper.EmptyMethod<ShipEjectionSystem>("OnPressInteract");
			QSBCore.Helper.HarmonyHelper.AddPostfix<ShipDamageController>("Awake", typeof(DeathPatches), nameof(DamageController_Exploded));
			QSBCore.Helper.HarmonyHelper.AddPrefix<DestructionVolume>("VanishShip", typeof(DeathPatches), nameof(DestructionVolume_VanishShip));
		}

		public static bool PreFinishDeathSequence(DeathType deathType)
		{
			if (RespawnOnDeath.Instance == null)
			{
				return true;
			}

			if (RespawnOnDeath.Instance.AllowedDeathTypes.Contains(deathType))
			{
				DebugLog.DebugWrite($"Allowing death of {deathType}");
				return true;
			}

			DebugLog.DebugWrite($"Not allowing death of {deathType}");

			RespawnOnDeath.Instance.ResetShip();
			RespawnOnDeath.Instance.ResetPlayer();
			return false;
		}

		public static void BroadcastDeath(DeathType deathType) 
			=> GlobalMessenger<DeathType>.FireEvent(EventNames.QSBPlayerDeath, deathType);

		public static void DamageController_Exploded(ref bool ____exploded)
		{
			____exploded = true;
		}

		public static IEnumerable<CodeInstruction> ReturnNull(IEnumerable<CodeInstruction> instructions)
		{
			return new List<CodeInstruction>
			{
				new CodeInstruction(OpCodes.Ldnull),
				new CodeInstruction(OpCodes.Ret)
			};
		}

		public static bool DestructionVolume_VanishShip(DeathType ____deathType)
		{
			// Don't want to vanish the ship - that disables the ship completely
			if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || PlayerState.AtFlightConsole())
			{
				Locator.GetDeathManager().KillPlayer(____deathType);
			}
			return false;
		}
	}
}