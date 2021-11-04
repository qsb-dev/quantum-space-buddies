using HarmonyLib;
using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;

namespace QSB.AnglerFish.WorldObjects {
    public class QSBAngler : WorldObject<AnglerfishController> {
        public override void Init(AnglerfishController attachedObject, int id) {
            ObjectId = id;
            AttachedObject = attachedObject;
        }

        public void HandleEvent() {
            // DebugLog.ToAll($"{AttachedObject} {ObjectId}: sector occupants changed...");
            // DebugLog.ToConsole(AttachedObject._sector.GetOccupants().Join());
            if (QNetworkServer.active) {
                QSBEventManager.FireEvent(EventNames.QSBAnglerResync, ObjectId);
            }
        }
    }
}
