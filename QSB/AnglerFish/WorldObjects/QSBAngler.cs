using QSB.Events;
using QSB.WorldSync;

namespace QSB.AnglerFish.WorldObjects {
    public class QSBAngler : WorldObject<AnglerfishController> {
        public override void Init(AnglerfishController attachedObject, int id) {
            ObjectId = id;
            AttachedObject = attachedObject;

            attachedObject.OnAnglerUnsuspended += _ => OnSectorEnterLeave(true);
            attachedObject.OnAnglerSuspended += _ => OnSectorEnterLeave(false);
        }

        private void OnSectorEnterLeave(bool entered) =>
            QSBEventManager.FireEvent(EventNames.QSBAnglerResync, ObjectId, AttachedObject.GetSector().name, entered);
    }
}
