using OWML.Common;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools
{
    public class QSBProbe : MonoBehaviour
    {
        private uint _attachedNetId;

        public void Init(uint netid)
        {
            _attachedNetId = netid;
        }

        void Start()
        {
            gameObject.SetActive(false);
        }

        public void Activate()
        {
            DebugLog.ToConsole($"Activating player {_attachedNetId}'s probe.", MessageType.Info);
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            DebugLog.ToConsole($"Deactivating player {_attachedNetId}'s probe.", MessageType.Info);
            gameObject.SetActive(false);
        }
    }
}
