using System.Linq;
using OWML.Common;
using QSB.Tools;
using QSB.Utility;
using UnityEngine;

namespace QSB
{
    public class PlayerInfo
    {
        public uint NetId { get; }
        public GameObject Body { get; set; }
        public GameObject Camera { get; set; }
        public GameObject ProbeBody { get; set; }
        public QSBProbe Probe { get; set; }
        public QSBFlashlight FlashLight => Camera.GetComponentInChildren<QSBFlashlight>();
        public QSBTool Signalscope => GetToolByType(ToolType.Signalscope);
        public QSBTool Translator => GetToolByType(ToolType.Translator);
        public QSBTool ProbeLauncher => GetToolByType(ToolType.ProbeLauncher);
        public string Name { get; set; }
        public bool IsReady { get; set; }
        public Transform ReferenceSector { get; set; }
        public State State { get; private set; }

        public PlayerInfo(uint id)
        {
            NetId = id;
        }

        public void UpdateState(State state, bool value)
        {
            DebugLog.ToConsole($"Updating player state {NetId}.{state}.{value}", MessageType.Info);
            var states = State;
            if (value)
            {
                FlagsHelper.Set(ref states, state);
            }
            else
            {
                FlagsHelper.Unset(ref states, state);
            }
            State = states;
        }

        private QSBTool GetToolByType(ToolType type)
        {
            DebugLog.ToConsole($"Getting tool {type} for player id {NetId}");
            return Camera.GetComponentsInChildren<QSBTool>().First(x => x.Type == type);
        }
    }
}