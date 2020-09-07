using QSB.Events;
using QSB.WorldSync;

namespace QSB.OrbSync
{
    public class QSBOrbSlot : WorldObject
    {
        public NomaiInterfaceSlot InterfaceSlot { get; private set; }

        private bool _initialized;

        public void Init(NomaiInterfaceSlot slot, int id)
        {
            ObjectId = id;
            InterfaceSlot = slot;
            _initialized = true;
        }

        public void HandleEvent(bool state)
        {
            if (QSB.HasWokenUp)
            {
                GlobalMessenger<int, bool>.FireEvent(EventNames.QSBOrbSlot, ObjectId, state);
            }
        }

        public void SetState(bool state)
        {
            if (!_initialized)
            {
                return;
            }
            if (state)
            {
                WorldRegistry.RaiseEvent(InterfaceSlot, "OnSlotActivated");
                return;
            }
            WorldRegistry.RaiseEvent(InterfaceSlot, "OnSlotDeactivated");
        }
    }
}
