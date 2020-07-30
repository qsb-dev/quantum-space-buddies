using System.Linq;
using OWML.Common;
using QSB.Animation;
using QSB.Utility;
using UnityEngine;

namespace QSB
{
    public class PlayerInfo
    {
        public uint NetId { get; }
        public GameObject Body { get; set; }
        public GameObject Camera { get; set; }
        public QSBFlashlight FlashLight => Camera.GetComponentInChildren<QSBFlashlight>();
        public string Name { get; set; }
        public bool IsReady { get; set; }
        public Transform ReferenceSector { get; set; }
        public State State { get; set; }

        public QSBTool Signalscope
        {
            get
            {
                var tools = Camera.GetComponentsInChildren<QSBTool>();
                if (tools.Length == 0)
                {
                    DebugLog.ToConsole("Error - Zero items in QSBTool list while trying to get Signalscope", MessageType.Error);
                    return null;
                }
                return tools.First(x => x.Type == ToolType.Signalscope);
            }
        }

        public PlayerInfo(uint id)
        {
            NetId = id;
        }

        public void UpdateState(State state, bool value)
        {
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

    }
}