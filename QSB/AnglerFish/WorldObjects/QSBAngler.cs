using QSB.Events;
using QSB.WorldSync;
using QuantumUNET;

namespace QSB.AnglerFish.WorldObjects {
    public class QSBAngler : WorldObject<AnglerfishController> {
        public override void Init(AnglerfishController attachedObject, int id) {
            ObjectId = id;
            AttachedObject = attachedObject;
        }

        public void HandleEvent() {
            if (QNetworkServer.active)
                QSBEventManager.FireEvent(EventNames.QSBAnglerResync, ObjectId);
        }
    }
}
