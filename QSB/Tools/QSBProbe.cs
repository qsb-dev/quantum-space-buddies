using OWML.Common;
using QSB.TransformSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools
{
    public class QSBProbe : MonoBehaviour
    {
        public GameObject Body { get; private set; }

        private PlayerInfo _player;
        private PlayerProbeSync _probeSync;

        public void Init(GameObject body, PlayerInfo player,  PlayerProbeSync playerProbeSync)
        {
            Body = body;
            _player = player;
            _probeSync = playerProbeSync;
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void Activate()
        {
            DebugLog.ToConsole($"Activating {_player.Name}'s probe.", MessageType.Info);
            gameObject.SetActive(true);
            Reset();
        }

        public void Deactivate()
        {
            DebugLog.ToConsole($"Deactivating {_player.Name}'s probe.", MessageType.Info);
            Reset();
            gameObject.SetActive(false);
        }

        public void Reset()
        {
            _probeSync.TeleportToPlayer(_player);
        }
    }
}
