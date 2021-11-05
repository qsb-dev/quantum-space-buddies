using System.Linq;
using QSB.AnglerFish.WorldObjects;
using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;
using EventType = QSB.Events.EventType;

namespace QSB.AnglerFish.Events {
    /// resync position when it's sector occupants changes
    internal class AnglerResyncEvent : QSBEvent<AnglerResyncMessage> {
        public override EventType Type => EventType.AnglerResync;

        public override void SetupListener() =>
            GlobalMessenger<int, string, bool>.AddListener(EventNames.QSBAnglerResync, Handler);

        public override void CloseListener() =>
            GlobalMessenger<int, string, bool>.RemoveListener(EventNames.QSBAnglerResync, Handler);

        private void Handler(int anglerId, string sector, bool entered) {
            SendEvent(new AnglerResyncMessage {
                type = AnglerResyncMessage.Type.SectorEnterLeave,

                ObjectId = anglerId,
                sector = sector,
                entered = entered
            });
        }

        private void BroadcastTransform(uint fromId, int anglerId) {
            var angler = QSBWorldSync.GetWorldFromId<QSBAngler>(anglerId).AttachedObject;
            var transform = angler._anglerBody.transform;
            // coords are relative to bramble
            var reference = angler._brambleBody.transform;
            SendEvent(new AnglerResyncMessage {
                type = AnglerResyncMessage.Type.TransformBroadcast,

                ObjectId = anglerId,
                AboutId = fromId,
                pos = reference.InverseTransformPoint(transform.position),
                rot = reference.InverseTransformRotation(transform.rotation),
            });
        }

        public override void OnReceiveLocal(bool isHost, AnglerResyncMessage message) => OnReceiveRemote(isHost, message);

        public override void OnReceiveRemote(bool isHost, AnglerResyncMessage message) {
            switch (message.type) {
                case AnglerResyncMessage.Type.SectorEnterLeave when isHost:
                    DebugLog.ToAll(message.ToString());
                    if (message.entered) {
                        var id = message.FromId;
                        // use an existing occupant's transform if there is one
                        var pair = AnglerManager.sectorOccupants.FirstOrDefault(p => p.Value == message.sector);
                        if (pair.Value != null) id = pair.Key;

                        // request transform
                        SendEvent(new AnglerResyncMessage {
                            type = AnglerResyncMessage.Type.TransformRequest,

                            ObjectId = message.ObjectId,
                            AboutId = id
                        });

                        AnglerManager.sectorOccupants[message.FromId] = message.sector;
                    } else {
                        AnglerManager.sectorOccupants.Remove(message.FromId);

                        // if player is the last one out
                        if (AnglerManager.sectorOccupants.Count(pair => pair.Value == message.sector) == 0)
                            BroadcastTransform(message.FromId, message.ObjectId);
                    }

                    break;


                case AnglerResyncMessage.Type.TransformRequest when message.AboutId == LocalPlayerId:
                    DebugLog.ToAll(message.ToString());
                    BroadcastTransform(LocalPlayerId, message.ObjectId);
                    break;


                case AnglerResyncMessage.Type.TransformBroadcast when message.AboutId != LocalPlayerId:
                    DebugLog.ToAll(message.ToString());
                    var angler = QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId).AttachedObject;
                    var reference = angler._brambleBody.transform;
                    // convert to transform relative to us
                    angler.transform.SetPositionAndRotation(
                        reference.TransformPoint(message.pos),
                        reference.TransformRotation(message.rot)
                    );
                    break;
            }
        }
    }
}
