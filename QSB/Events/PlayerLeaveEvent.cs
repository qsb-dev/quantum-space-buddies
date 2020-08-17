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
            GlobalMessenger<uint, uint[]>.AddListener(EventNames.QSBPlayerLeave, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<uint, uint[]>.RemoveListener(EventNames.QSBPlayerLeave, Handler);
        }

        private void Handler(uint id, uint[] objects) => SendEvent(CreateMessage(id, objects));

        private PlayerLeaveMessage CreateMessage(uint id, uint[] objects) => new PlayerLeaveMessage
        {
            FromId = LocalPlayerId,
            AboutId = id,
            ObjectIds = objects
        };

        public override void OnReceiveRemote(PlayerLeaveMessage message)
        {
            var playerName = PlayerRegistry.GetPlayer(message.AboutId).Name;
            DebugLog.ToConsole($"{playerName} disconnected.", OWML.Common.MessageType.Info);
            PlayerRegistry.RemovePlayer(message.AboutId);
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
                PlayerRegistry.TransformSyncs.Remove(transformSync);
                if (transformSync.SyncedTransform != null)
                {
                    Object.Destroy(transformSync.SyncedTransform.gameObject);
                }
            }
            Object.Destroy(component.gameObject);
        }
    }
}
