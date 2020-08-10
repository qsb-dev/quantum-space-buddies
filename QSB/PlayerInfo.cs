using System.Linq;
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
        public State State { get; set; }

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

        public void UpdateStateObjects()
        {
            if (!QSB.WokenUp)
            {
                DebugLog.ToConsole("Tried to update state objects, but local player hasn't woken up!");
                return;
            }
            DebugLog.ToConsole($"Updating state objects for player {NetId}");
            FlashLight.UpdateState(FlagsHelper.IsSet(State, State.Flashlight));
            Translator.ChangeEquipState(FlagsHelper.IsSet(State, State.Translator));
            ProbeLauncher.ChangeEquipState(FlagsHelper.IsSet(State, State.ProbeLauncher));
            Signalscope.ChangeEquipState(FlagsHelper.IsSet(State, State.Signalscope));

            if (FlagsHelper.IsSet(State, State.Suit))
            {
                PlayerRegistry.GetAnimationSync(NetId).SuitUp();
            }
            else
            {
                PlayerRegistry.GetAnimationSync(NetId).SuitDown();
            }
        }

        public bool GetState(State state)
        {
            return FlagsHelper.IsSet(State, state);
        }

        private QSBTool GetToolByType(ToolType type)
        {
            return Camera.GetComponentsInChildren<QSBTool>().First(x => x.Type == type);
        }
    }
}