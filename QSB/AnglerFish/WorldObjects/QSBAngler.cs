using QSB.Events;
using QSB.WorldSync;

namespace QSB.AnglerFish.WorldObjects {
    public class QSBAngler : WorldObject<AnglerfishController> {
        public override void Init(AnglerfishController attachedObject, int id) {
            ObjectId = id;
            AttachedObject = attachedObject;

            // delay to prevent weird sector exit calls when a client joins
            QSBCore.UnityEvents.FireInNUpdates(() => {
                AttachedObject.OnAnglerUnsuspended += OnUnsuspended;
                AttachedObject.OnAnglerSuspended += OnSuspended;
            }, 10);
        }

        public override void OnRemoval() {
            AttachedObject.OnAnglerUnsuspended -= OnUnsuspended;
            AttachedObject.OnAnglerSuspended -= OnSuspended;
        }

        private void OnSuspended(AnglerfishController.AnglerState _) => OnSectorEnterLeave(false);
        private void OnUnsuspended(AnglerfishController.AnglerState _) => OnSectorEnterLeave(true);

        private void OnSectorEnterLeave(bool entered) =>
            QSBEventManager.FireEvent(EventNames.QSBAnglerResync, ObjectId, AttachedObject.GetSector().name, entered);
    }
}
