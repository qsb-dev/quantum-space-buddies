using System.Linq;
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
        public string Name { get; set; }
        public bool IsReady { get; set; }
        public Transform ReferenceSector { get; set; }
        public State State { get; private set; }
        public QSBFlashlight FlashLight => Camera.GetComponentInChildren<QSBFlashlight>();
        public QSBTool Signalscope => Camera.GetComponentsInChildren<QSBTool>().First(x => x.Type == ToolType.Signalscope);

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