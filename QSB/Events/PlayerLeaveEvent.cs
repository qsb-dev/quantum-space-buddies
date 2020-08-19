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
            DebugLog.ToConsole("Destroying object " + objectId);
            var components = Object.FindObjectsOfType<NetworkBehaviour>()
                .Where(x => x.netId.Value == objectId);
            foreach (var component in components)
            {
                DebugLog.ToConsole("* For object " + component.GetType().Name);
                if (component == null)
                {
                    DebugLog.ToConsole("    * Component is null!");
                    return;
                }
                var transformSync = component.GetComponent<TransformSync.TransformSync>();

                if (transformSync != null)
                {
                    DebugLog.ToConsole("    * TS is not null - removing from list");
                    PlayerRegistry.TransformSyncs.Remove(transformSync);
                    if (transformSync.SyncedTransform != null)
                    {
                        DebugLog.ToConsole("    * TS's ST is not null - destroying");
                        Object.Destroy(transformSync.SyncedTransform.gameObject);
                    }
                }
                DebugLog.ToConsole("    * Destroying component's gameobject " + component.gameObject.name);
                Object.Destroy(component.gameObject);
            }
        }
    }
}
