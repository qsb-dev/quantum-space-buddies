using OWML.Common;
using QSB.ConversationSync;
using QSB.DeathSync;
using QSB.ElevatorSync;
using QSB.OrbSync;
using QSB.TimeSync;
using QSB.Tools;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Patches
{
    public delegate void PatchEvent(QSBPatchTypes type);

    public static class QSBPatchManager
    {
        public static List<QSBPatch> _patchList = new List<QSBPatch>();

        public static event PatchEvent OnPatchType;

        public static void Init()
        {
            _patchList = new List<QSBPatch>
            {
                new ConversationPatches(),
                new DeathPatches(),
                new ElevatorPatches(),
                new OrbPatches(),
                new WakeUpPatches(),
                new ProbePatches()
            };

            DebugLog.DebugWrite("Patch Manager ready.", MessageType.Success);
        }

        public static void DoPatchType(QSBPatchTypes type)
        {
            OnPatchType(type);
            DebugLog.DebugWrite($"Patch block {Enum.GetName(typeof(QSBPatchTypes), type)}", MessageType.Info);
            foreach (var patch in _patchList.Where(x => x.Type == type))
            {
                DebugLog.DebugWrite($" - Patching in {patch.GetType().Name}", MessageType.Info);
                patch.DoPatches();
            }
        }
    }
}
