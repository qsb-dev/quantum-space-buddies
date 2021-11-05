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

        private void Handler(int anglerId, string sector, bool entered) =>
            SendEvent(AnglerResyncMessage.SectorEnterLeave(
                anglerId,
                sector,
                entered
            ));

        private void BroadcastTransform(uint fromId, int anglerId) {
            var angler = QSBWorldSync.GetWorldFromId<QSBAngler>(anglerId).AttachedObject;
            var transform = angler._anglerBody.transform;
            // coords are relative to bramble
            var reference = angler._brambleBody.transform;
            SendEvent(AnglerResyncMessage.TransformBroadcast(
                anglerId,
                fromId,
                reference.InverseTransformPoint(transform.position),
                reference.InverseTransformRotation(transform.rotation)
            ));
        }

        public override void OnReceiveLocal(bool isHost, AnglerResyncMessage message) => OnReceiveRemote(isHost, message);

        public override void OnReceiveRemote(bool isHost, AnglerResyncMessage message) {
            switch (message.type) {
                case AnglerResyncMessage.Type.SectorEnterLeave when isHost:
                    DebugLog.ToAll(message.ToString());

                    var occupants = AnglerManager.GetSectorOccupants(message.sector);
                    if (message.entered) {
                        // if there's already occupants in this sector, get transform from them
                        if (occupants.Any())
                            SendEvent(AnglerResyncMessage.TransformRequest(
                                message.ObjectId,
                                occupants.First()
                            ));

                        occupants.Add(message.FromId);
                    } else {
                        occupants.Remove(message.FromId);

                        // if player is the last one out, sync transform one final time
                        if (!occupants.Any())
                            BroadcastTransform(message.FromId, message.ObjectId);
                    }

                    // DebugLog.ToAll(AnglerManager.sectorOccupants.Join(pair => $"{pair.Key}: {pair.Value.Join()}", "\n"));
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
