using QSB.Animation;
using QSB.Instruments;
using QSB.Tools;
using QSB.TransformSync;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Player
{
    public class PlayerInfo
    {
        public uint PlayerId { get; }
        public string Name { get; set; }
        public bool IsReady { get; set; }
        public PlayerHUDMarker HudMarker { get; set; }
        public State State { get; set; }

        // Body Objects
        public GameObject Camera { get; set; }
        public GameObject Body { get; set; }

        // Tools
        public GameObject ProbeBody { get; set; }
        public QSBProbe Probe { get; set; }
        public QSBFlashlight FlashLight => Camera?.GetComponentInChildren<QSBFlashlight>();
        public QSBTool Signalscope => GetToolByType(ToolType.Signalscope);
        public QSBTool Translator => GetToolByType(ToolType.Translator);
        public QSBTool ProbeLauncher => GetToolByType(ToolType.ProbeLauncher);

        // Conversation
        public int CurrentDialogueID { get; set; }
        public GameObject CurrentDialogueBox { get; set; }

        // Instruments
        public InstrumentType CurrentInstrument { get; set; }
        public bool PlayingInstrument => CurrentInstrument != InstrumentType.NONE;

        public PlayerInfo(uint id)
        {
            DebugLog.DebugWrite($"Creating PlayerInfo with id {id}");
            PlayerId = id;
            CurrentDialogueID = -1;
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
            if (OWInput.GetInputMode() == InputMode.None)
            {
                return;
            }
            FlashLight?.UpdateState(FlagsHelper.IsSet(State, State.Flashlight));
            Translator?.ChangeEquipState(FlagsHelper.IsSet(State, State.Translator));
            ProbeLauncher?.ChangeEquipState(FlagsHelper.IsSet(State, State.ProbeLauncher));
            Signalscope?.ChangeEquipState(FlagsHelper.IsSet(State, State.Signalscope));
            QSB.Helper.Events.Unity.RunWhen(() => QSBPlayerManager.GetSyncObject<AnimationSync>(PlayerId) != null,
                () => QSBPlayerManager.GetSyncObject<AnimationSync>(PlayerId).SetSuitState(FlagsHelper.IsSet(State, State.Suit)));
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