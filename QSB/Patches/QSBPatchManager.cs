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
using System.Text;

namespace QSB.Patches
{
    public static class QSBPatchManager
    {
        public static List<QSBPatch> _patchList = new List<QSBPatch>();

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

            DebugLog.DebugWrite($"Patch manager ready.", MessageType.Success);
        }

        public static void DoPatchType(QSBPatchTypes type)
        {
            DebugLog.DebugWrite($"Setting up patch block {Enum.GetName(typeof(QSBPatchTypes), type)}", MessageType.Info);
            foreach (var patch in _patchList.Where(x => x.Type == type))
            {
                DebugLog.DebugWrite($" - Doing patches for {patch.GetType().Name}", MessageType.Info);
                patch.DoPatches();
            }
        }
    }
}
