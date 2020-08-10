using QSB.Messaging;
using QSB.Utility;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerLeaveEvent : QSBEvent<PlayerLeaveMessage>
    {
        public override MessageType Type => MessageType.PlayerLeave;

        public override void SetupListener()
        {
            GlobalMessenger<uint, uint[]>.AddListener("QSBPlayerLeave", (id, objects) => SendEvent(CreateMessage(id, objects)));
        }

        private PlayerLeaveMessage CreateMessage(uint id, uint[] objects) => new PlayerLeaveMessage
        {
            SenderId = id,
            ObjectIds = objects
        };

        public override void OnReceiveRemote(PlayerLeaveMessage message)
        {
            var playerName = PlayerRegistry.GetPlayer(message.SenderId).Name;
            DebugLog.ToAll(playerName, "disconnected.");
            PlayerRegistry.RemovePlayer(message.SenderId);
            foreach (var objectId in message.ObjectIds)
            {
                DestroyObject(objectId);
            }
        }

        private void DestroyObject(uint objectId)
        {
            var component = Object.FindObjectsOfType<NetworkBehaviour>()
                .FirstOrDefault(x => x.netId.Value == objectId);
            if (component == null)
            {
                return;
            }
            var transformSync = component.GetComponent<TransformSync.TransformSync>();
            if (transformSync != null)
            {
                Object.Destroy(transformSync.SyncedTransform.gameObject);
            }
            Object.Destroy(component.gameObject);
        }
    }
}
