using System.Linq;
using QSB.Messaging;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerLeave : NetworkBehaviour
    {
        private MessageHandler<LeaveMessage> _leaveHandler;

        private void Awake()
        {
            _leaveHandler = new MessageHandler<LeaveMessage>();
            _leaveHandler.OnClientReceiveMessage += OnClientReceiveMessage;
        }

        public void Leave(uint playerId, uint shipId)
        {
            var message = new LeaveMessage
            {
                PlayerName = PlayerJoin.PlayerNames[playerId],
                SenderId = playerId,
                ShipId = shipId
            };
            _leaveHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(LeaveMessage message)
        {
            DebugLog.All(message.PlayerName, "left");
            PlayerJoin.PlayerNames.Remove(message.SenderId);
            DestroySyncedObject<PlayerTransformSync>(message.SenderId);
            DestroySyncedObject<ShipTransformSync>(message.ShipId);
        }

        private void DestroySyncedObject<T>(uint id) where T : TransformSync.TransformSync
        {
            var transformSync = GameObject.FindObjectsOfType<T>()
                .FirstOrDefault(x => x.netId.Value == id);
            if (transformSync == null)
            {
                return;
            }
            Destroy(transformSync.SyncedTransform.gameObject);
        }

    }
}
