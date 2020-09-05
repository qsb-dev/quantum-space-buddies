using QSB.Events;
using QSB.WorldSync;
using System;
using System.Linq;
using System.Reflection;

namespace QSB.OrbSync
{
    public class QSBOrbSlot : WorldObject
    {
        private NomaiInterfaceSlot _interfaceSlot;

        public void Init(NomaiInterfaceSlot slot, int id)
        {
            ObjectId = id;
            _interfaceSlot = slot;
            _interfaceSlot.OnSlotActivated += (slotInstance) => HandleEvent(true);
            _interfaceSlot.OnSlotDeactivated += (slotInstance) => HandleEvent(false);
        }

        private void HandleEvent(bool state)
        {
            GlobalMessenger<int, bool>.FireEvent(EventNames.QSBOrbSlot, ObjectId, state);
        }

        public void SetState(bool state)
        {
            if (state)
            {
                RaiseEvent(_interfaceSlot, "OnSlotActivated");
                return;
            }
            RaiseEvent(_interfaceSlot, "OnSlotDeactivated");
        }

        private static void RaiseEvent(object instance, string eventName)
        {
            var type = instance.GetType();
            var staticFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var fieldInfo = type.GetField(eventName, staticFlags);
            var multDelegate = fieldInfo.GetValue(instance) as MulticastDelegate;
            var delegateList = multDelegate.GetInvocationList().ToList();
            delegateList.ForEach(x => x.DynamicInvoke(instance));
        }
    }
}
