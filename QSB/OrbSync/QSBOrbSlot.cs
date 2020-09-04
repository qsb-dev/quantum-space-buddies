using QSB.Events;
using QSB.WorldSync;
using System;
using System.Reflection;
using UnityEngine.Networking;

namespace QSB.OrbSync
{
    public class QSBOrbSlot : WorldObject
    {
        private NomaiInterfaceSlot _interfaceSlot;

        public void Init(NomaiInterfaceSlot slot, int id)
        {
            ObjectId = id;
            _interfaceSlot = slot;

            _interfaceSlot.OnSlotActivated += (slotInstance) => HandleEvent(slotInstance, true);
            _interfaceSlot.OnSlotDeactivated += (slotInstance) => HandleEvent(slotInstance, false);
        }

        private void HandleEvent(NomaiInterfaceSlot instance, bool state)
        {
            if (NetworkServer.active)
            {
                GlobalMessenger<int, bool>.FireEvent(EventNames.QSBOrbSlot, ObjectId, state);
            }
        }

        public void SetState(bool state)
        {
            if (state)
            {
                RaiseEvent(_interfaceSlot, "OnSlotActivated");
            }
            else
            {
                RaiseEvent(_interfaceSlot, "OnSlotDeactivated");
            }
        }

        private static void RaiseEvent(object instance, string eventName)
        {
            var type = instance.GetType();
            var staticFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var fieldInfo = type.GetField(eventName, staticFlags);
            var multDelegate = fieldInfo.GetValue(instance) as MulticastDelegate;
            var delegateList = multDelegate.GetInvocationList();
            foreach (var delegateMethod in delegateList)
            {
                delegateMethod.DynamicInvoke(instance);
            }
        }
    }
}
