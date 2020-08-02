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
        public Vector3 Position => Body.transform.position;
        public GameObject Camera { get; set; }
        public QSBFlashlight FlashLight => Camera.GetComponentInChildren<QSBFlashlight>();
        public QSBTool Signalscope => GetToolByType(ToolType.Signalscope);
        public QSBTool Translator => GetToolByType(ToolType.Translator);

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                DebugLog.UpdateInfoCanvas(NetId, InfoState.Name, _name);
            }
        }

        public bool IsReady { get; set; }
        private Transform _referenceSector;
        public Transform ReferenceSector
        {
            get
            {
                return _referenceSector;
            }

            set
            {
                _referenceSector = value;
                DebugLog.UpdateInfoCanvas(NetId, InfoState.Sector, _referenceSector.name);
            }
        }
        public State State { get; private set; }

        public PlayerInfo(uint id)
        {
            NetId = id;
            DebugLog.UpdateInfoCanvas(NetId, InfoState.ID, id);
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

        private QSBTool GetToolByType(ToolType type)
        {
            return Camera.GetComponentsInChildren<QSBTool>().First(x => x.Type == type);
        }

    }
}