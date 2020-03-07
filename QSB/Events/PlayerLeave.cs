using System.Linq;
using QSB.Messaging;
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

        public void Leave(uint playerId, uint[] objectIds)
        {
            var message = new LeaveMessage
            {
                PlayerName = PlayerJoin.PlayerNames[playerId],
                SenderId = playerId,
                ObjectIds = objectIds
            };
            _leaveHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(LeaveMessage message)
        {
            DebugLog.All(message.PlayerName, "left");
            PlayerJoin.PlayerNames.Remove(message.SenderId);
            foreach (var objectId in message.ObjectIds)
            {
                DestroyObject(objectId);
            }
        }

        private void DestroyObject(uint objectId)
        {
            var component = GameObject.FindObjectsOfType<NetworkBehaviour>()
                .FirstOrDefault(x => x.netId.Value == objectId);
            if (component == null)
            {
                return;
            }
            var transformSync = component.GetComponent<TransformSync.TransformSync>();
            if (transformSync != null)
            {
                Destroy(transformSync.SyncedTransform.gameObject);
            }
            Destroy(component.gameObject);
        }

    }
}
