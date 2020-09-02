using QSB.Animation;
using QSB.Tools;
using QSB.TransformSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB
{
    public class PlayerInfo
    {
        public uint NetId { get; }
        public GameObject Camera { get; set; }
        public GameObject ProbeBody { get; set; }
        public QSBProbe Probe { get; set; }
        public QSBFlashlight FlashLight => Camera?.GetComponentInChildren<QSBFlashlight>();
        public QSBTool Signalscope => GetToolByType(ToolType.Signalscope);
        public QSBTool Translator => GetToolByType(ToolType.Translator);
        public QSBTool ProbeLauncher => GetToolByType(ToolType.ProbeLauncher);
        public PlayerHUDMarker HudMarker { get; set; }
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
            //DebugLog.DebugWrite($"State of player {NetId} is now : {Environment.NewLine}" +
            //    $"{DebugLog.GenerateTable(Enum.GetNames(typeof(State)).ToList(), FlagsHelper.FlagsToListSet(State))}");
        }

        public void UpdateStateObjects()
        {
            if (OWInput.GetInputMode() == InputMode.None)
            {
                return;
            }
            FlashLight?.UpdateState(FlagsHelper.IsSet(State, State.Flashlight));
            Translator?.ChangeEquipState(FlagsHelper.IsSet(State, State.Translator));
            ProbeLauncher?.ChangeEquipState(FlagsHelper.IsSet(State, State.ProbeLauncher));
            Signalscope?.ChangeEquipState(FlagsHelper.IsSet(State, State.Signalscope));
            QSB.Helper.Events.Unity.RunWhen(() => PlayerRegistry.GetSyncObject<AnimationSync>(NetId) != null,
                () => PlayerRegistry.GetSyncObject<AnimationSync>(NetId).SetSuitState(FlagsHelper.IsSet(State, State.Suit)));
        }

        public bool GetState(State state)
        {
            return FlagsHelper.IsSet(State, state);
        }

        private QSBTool GetToolByType(ToolType type)
        {
            return Camera?.GetComponentsInChildren<QSBTool>()
                .FirstOrDefault(x => x.Type == type);
        }
    }
}