using OWML.Common;
using QSB.ConversationSync;
using QSB.DeathSync;
using QSB.ElevatorSync;
using QSB.FrequencySync;
using QSB.LogSync;
using QSB.OrbSync;
using QSB.QuantumSync;
using QSB.SpiralSync.Patches;
using QSB.TimeSync;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Patches
{
	public delegate void PatchEvent(QSBPatchTypes type);

	public static class QSBPatchManager
	{
		public static event PatchEvent OnPatchType;

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
				new ServerQuantumStateChangePatches(),
				new ClientQuantumStateChangePatches(),
				new FrequencyPatches(),
				new SpiralPatches()
			};

			DebugLog.DebugWrite("Patch Manager ready.", MessageType.Success);
		}

		public static void DoPatchType(QSBPatchTypes type)
		{
			OnPatchType?.Invoke(type);
			DebugLog.DebugWrite($"Patch block {Enum.GetName(typeof(QSBPatchTypes), type)}", MessageType.Info);
			foreach (var patch in _patchList.Where(x => x.Type == type))
			{
				DebugLog.DebugWrite($" - Patching in {patch.GetType().Name}", MessageType.Info);
				patch.DoPatches();
			}
		}
	}
}