using QSB.Events;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine.Networking;

namespace QSB.OrbSync
{
    public class OrbUserEvent : QSBEvent<WorldObjectMessage>
    {
        public override EventType Type => EventType.OrbUser;

        public override void SetupListener()
        {
            GlobalMessenger<int>.AddListener(EventNames.QSBOrbUser, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<int>.RemoveListener(EventNames.QSBOrbUser, Handler);
        }

        private void Handler(int id) => SendEvent(CreateMessage(id));

        private WorldObjectMessage CreateMessage(int id) => new WorldObjectMessage
        {
            AboutId = LocalPlayerId,
            ObjectId = id
        };

        public override void OnServerReceive(WorldObjectMessage message)
        {
            var fromPlayer = (NetworkServer.connections.First(x => x.playerControllers[0].gameObject.GetComponent<PlayerTransformSync>().netId.Value == message.FromId));
            DebugLog.DebugWrite($"[S] Setting orb {message.ObjectId} to auth id {message.FromId}");
            var orbIdentity = WorldRegistry.OrbList
                .First(x => x.AttachedOrb == WorldRegistry.OldOrbList[message.ObjectId])
                .GetComponent<NetworkIdentity>();
            orbIdentity.RemoveClientAuthority(orbIdentity.clientAuthorityOwner);
            orbIdentity.AssignClientAuthority(fromPlayer);
        }

        public override void OnReceiveLocal(WorldObjectMessage message)
        {
            if (NetworkServer.active)
            {
                OnServerReceive(message);
            }
        }
    }
}
