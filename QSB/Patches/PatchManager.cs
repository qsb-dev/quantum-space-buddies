using OWML.Common;
using QSB.ConversationSync.Patches;
using QSB.DeathSync.Patches;
using QSB.ElevatorSync.Patches;
using QSB.FrequencySync.Patches;
using QSB.LogSync.Patches;
using QSB.OrbSync.Patches;
using QSB.QuantumSync.Patches;
using QSB.TimeSync.Patches;
using QSB.TranslationSync.Patches;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Patches
{
	public static class PatchManager
	{
		public static event Action<PatchType> OnPatchType;
		public static event Action<PatchType> OnUnpatchType;

		private static List<QSBPatch> _patchList = new List<QSBPatch>();

		public static void Init()
		{
			_patchList = new List<QSBPatch>
			{
				new ConversationPatches(),
				new DeathPatches(),
				new ElevatorPatches(),
				new OrbPatches(),
				new WakeUpPatches(),
				new LogPatches(),
				new QuantumVisibilityPatches(),
				new ServerQuantumPatches(),
				new ClientQuantumPatches(),
				new FrequencyPatches(),
				new SpiralPatches(),
				new QuantumPatches()
			};

			DebugLog.DebugWrite("Patch Manager ready.", MessageType.Success);
		}

		public static void DoPatchType(PatchType type)
		{
			OnPatchType?.SafeInvoke(type);
			DebugLog.DebugWrite($"Patch block {Enum.GetName(typeof(PatchType), type)}", MessageType.Info);
			foreach (var patch in _patchList.Where(x => x.Type == type))
			{
				DebugLog.DebugWrite($" - Patching in {patch.GetType().Name}", MessageType.Info);
				patch.DoPatches();
			}
		}

		public static void DoUnpatchType(PatchType type)
		{
			OnUnpatchType?.SafeInvoke(type);
			DebugLog.DebugWrite($"Unpatch block {Enum.GetName(typeof(PatchType), type)}", MessageType.Info);
			foreach (var patch in _patchList.Where(x => x.Type == type))
			{
				DebugLog.DebugWrite($" - Unpatching in {patch.GetType().Name}", MessageType.Info);
				patch.DoUnpatches();
			}
		}
	}
}