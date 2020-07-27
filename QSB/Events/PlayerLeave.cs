using System.Linq;
using QSB.Messaging;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    /// <summary>
    /// Client-only-side component for managing player leaves.
    /// </summary>
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
                SenderId = playerId,
                ObjectIds = objectIds
            };
            _leaveHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(LeaveMessage message)
        {
            var playerName = PlayerRegistry.GetPlayerName(message.SenderId);
            DebugLog.ToAll(playerName, "disconnected.");
            PlayerRegistry.RemovePlayer(message.SenderId);
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
