using Harmony;
using QSB.Events;
using QSB.Patches;
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

		public override void DoUnpatches()
		{
			QSBCore.Helper.HarmonyHelper.Unpatch<DeathManager>("KillPlayer");
			QSBCore.Helper.HarmonyHelper.Unpatch<ShipDetachableLeg>("Detach");
			QSBCore.Helper.HarmonyHelper.Unpatch<ShipDetachableModule>("Detach");
			QSBCore.Helper.HarmonyHelper.Unpatch<ShipEjectionSystem>("OnPressInteract");
			QSBCore.Helper.HarmonyHelper.Unpatch<ShipDamageController>("Awake");
			QSBCore.Helper.HarmonyHelper.Unpatch<DestructionVolume>("VanishShip");
		}

		public static bool PreFinishDeathSequence(DeathType deathType)
		{
			if (RespawnOnDeath.Instance == null)
			{
				return true;
			}

			if (RespawnOnDeath.Instance.AllowedDeathTypes.Contains(deathType))
			{
				return true;
			}

			RespawnOnDeath.Instance.ResetShip();
			RespawnOnDeath.Instance.ResetPlayer();
			return false;
		}

		public static void BroadcastDeath(DeathType deathType)
			=> QSBEventManager.FireEvent<DeathType>(EventNames.QSBPlayerDeath, deathType);

		public static void DamageController_Exploded(ref bool ____exploded)
			=> ____exploded = true;

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
			if (RespawnOnDeath.Instance == null)
			{
				return true;
			}

			if (PlayerState.IsInsideShip() || PlayerState.UsingShipComputer() || PlayerState.AtFlightConsole())
			{
				Locator.GetDeathManager().KillPlayer(____deathType);
			}
			// Ship is being destroyed, but player isn't in it.
			RespawnOnDeath.Instance.ResetShip();
			return false;
		}
	}
}