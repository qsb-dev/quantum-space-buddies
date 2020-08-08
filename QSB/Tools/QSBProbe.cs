using OWML.Common;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools
{
    public class QSBProbe : MonoBehaviour
    {
        private PlayerInfo _player;

        public void Init(PlayerInfo player)
        {
            _player = player;
        }

        public void Activate()
        {
            DebugLog.ToConsole($"Activating {_player.Name}'s probe.", MessageType.Info);
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            DebugLog.ToConsole($"Deactivating {_player.Name}'s probe.", MessageType.Info);
            gameObject.SetActive(false);
        }
    }
}
