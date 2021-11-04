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

        private void Handler(int id) {
            var angler = QSBWorldSync.GetWorldFromId<QSBAngler>(id).AttachedObject;
            var transform = angler._anglerBody.transform;
            var reference = angler._brambleBody.transform;
            // coords are relative to bramble
            SendEvent(new AnglerResyncMessage {
                AboutId = LocalPlayerId,
                ObjectId = id,
                pos = reference.InverseTransformPoint(transform.position),
                rot = reference.InverseTransformRotation(transform.rotation)
            });
        }

        public override void OnReceiveRemote(bool isHost, AnglerResyncMessage message) {
            var angler = QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId).AttachedObject;
            var reference = angler._brambleBody.transform;
            // convert to transform relative to us
            QSBWorldSync.GetWorldFromId<QSBAngler>(message.ObjectId).AttachedObject.transform.SetPositionAndRotation(
                reference.TransformPoint(message.pos),
                reference.TransformRotation(message.rot)
            );
        }
    }
}
