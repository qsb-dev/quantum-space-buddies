using HarmonyLib;
using OWML.Common;
using OWML.Utils;
using QSB.Animation.NPC.Patches;
using QSB.Animation.Patches;
using QSB.CampfireSync.Patches;
using QSB.ConversationSync.Patches;
using QSB.DeathSync.Patches;
using QSB.ElevatorSync.Patches;
using QSB.FrequencySync.Patches;
using QSB.GeyserSync.Patches;
using QSB.Inputs.Patches;
using QSB.ItemSync.Patches;
using QSB.LogSync.Patches;
using QSB.OrbSync.Patches;
using QSB.Player.Patches;
using QSB.PoolSync.Patches;
using QSB.QuantumSync.Patches;
using QSB.RoastingSync.Patches;
using QSB.ShipSync.Patches;
using QSB.StatueSync.Patches;
using QSB.TimeSync.Patches;
using QSB.Tools.ProbeLauncherTool.Patches;
using QSB.TranslationSync.Patches;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Patches
{
	public static class QSBPatchManager
	{
		public static event Action<QSBPatchTypes> OnPatchType;
		public static event Action<QSBPatchTypes> OnUnpatchType;

		private static List<QSBPatch> _patchList = new List<QSBPatch>();

		public static Harmony HarmonyInstance;

		public static void Init()
		{
			_patchList = new List<QSBPatch>
			{
				new ConversationPatches(),
				new DeathPatches(),
				new ElevatorPatches(),
				new OrbPatches(),
				new LogPatches(),
				new QuantumVisibilityPatches(),
				new ServerQuantumPatches(),
				new ClientQuantumPatches(),
				new FrequencyPatches(),
				new SpiralPatches(),
				new QuantumPatches(),
				new ItemPatches(),
				new StatuePatches(),
				new GeyserPatches(),
				new PoolPatches(),
				new CampfirePatches(),
				new RoastingPatches(),
				new PlayerPatches(),
				new PlayerAnimationPatches(),
				new CharacterAnimationPatches(),
				new ShipPatches(),
				new InputPatches(),
				new TimePatches(),
				new MapPatches(),
				new RespawnPatches(),
				new LauncherPatches()
			};

			HarmonyInstance = QSBCore.Helper.HarmonyHelper.GetValue<Harmony>("_harmony");

			DebugLog.DebugWrite("Patch Manager ready.", MessageType.Success);
		}

		public static void DoPatchType(QSBPatchTypes type)
		{
			OnPatchType?.SafeInvoke(type);
			DebugLog.DebugWrite($"Patch block {Enum.GetName(typeof(QSBPatchTypes), type)}", MessageType.Info);
			foreach (var patch in _patchList.Where(x => x.Type == type))
			{
				DebugLog.DebugWrite($" - Patching in {patch.GetType().Name}", MessageType.Info);
				try
				{
					patch.DoPatches();
				}
				catch (Exception ex)
				{
					DebugLog.DebugWrite($"Error while patching {patch.GetType().Name} :\r\n{ex}", MessageType.Error);
				}
			}
		}

		public static void DoUnpatchType(QSBPatchTypes type)
		{
			OnUnpatchType?.SafeInvoke(type);
			DebugLog.DebugWrite($"Unpatch block {Enum.GetName(typeof(QSBPatchTypes), type)}", MessageType.Info);
			foreach (var patch in _patchList.Where(x => x.Type == type))
			{
				DebugLog.DebugWrite($" - Unpatching in {patch.GetType().Name}", MessageType.Info);
				patch.DoUnpatches();
			}
		}
	}
}