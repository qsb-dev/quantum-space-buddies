using System;
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
            GlobalMessenger<int>.AddListener(EventNames.QSBAnglerResync, Handler);

        public override void CloseListener() =>
            GlobalMessenger<int>.RemoveListener(EventNames.QSBAnglerResync, Handler);

        private void Handler(int anglerId) {
            var angler = QSBWorldSync.GetWorldFromId<QSBAngler>(anglerId).AttachedObject;

            // DebugLog.ToAll($"{angler} {anglerId}: sector occupants changed...");
            // DebugLog.ToConsole(angler.GetSector().GetOccupants().Join());

            SendEvent(new AnglerResyncMessage {
                type = AnglerResyncMessage.Type.OccupantUpdate,

                sector = angler.GetSector().name,
                inside = angler.GetSector().ContainsAnyOccupants(
                    DynamicOccupant.Player | DynamicOccupant.Probe | DynamicOccupant.Ship)
            });
        }

        private void BroadcastTransform(int anglerId) {
            var angler = QSBWorldSync.GetWorldFromId<QSBAngler>(anglerId).AttachedObject;
            var transform = angler._anglerBody.transform;
            // coords are relative to bramble
            var reference = angler._brambleBody.transform;
            SendEvent(new AnglerResyncMessage {
                type = AnglerResyncMessage.Type.TransformBroadcast,

                ObjectId = anglerId,
                pos = reference.InverseTransformPoint(transform.position),
                rot = reference.InverseTransformRotation(transform.rotation),
            });
        }

        public override void OnReceiveLocal(bool isHost, AnglerResyncMessage message) => OnReceiveRemote(isHost, message);

        public override void OnReceiveRemote(bool isHost, AnglerResyncMessage message) {
            DebugLog.ToAll(message.ToString());
            switch (message.type) {
                case AnglerResyncMessage.Type.OccupantUpdate when isHost:
                    if (message.inside) {
                        // player already in this sector, do nothing
                        if (AnglerManager.sectorOccupants.ContainsKey(message.FromId)) return;

                        var id = message.FromId;
                        // use an existing occupant's transform if there is one
                        foreach (var pair in AnglerManager.sectorOccupants.Where(pair => pair.Value == message.sector)) {
                            id = pair.Key;
                            break;
                        }

                        // request transform
                        SendEvent(new AnglerResyncMessage {
                            type = AnglerResyncMessage.Type.TransformRequest,

                            AboutId = id,
                            ObjectId = message.ObjectId
                        });

                        AnglerManager.sectorOccupants[message.FromId] = message.sector;
                    } else {
                        var removed = AnglerManager.sectorOccupants.Remove(message.FromId);
                        // player already out of the sector, somehow, so do nothing
                        if (!removed) return;

                        // if player is the last one out
                        if (AnglerManager.sectorOccupants.Count(pair => pair.Value == message.sector) == 0)
                            BroadcastTransform(message.ObjectId);
                    }

                    break;


                case AnglerResyncMessage.Type.TransformRequest when message.AboutId == LocalPlayerId:
                    BroadcastTransform(message.ObjectId);
                    break;


                case AnglerResyncMessage.Type.TransformBroadcast when message.FromId != LocalPlayerId:
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
