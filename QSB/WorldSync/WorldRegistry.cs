using OWML.Common;
using QSB.OrbSync;
using QSB.TransformSync;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QSB.WorldSync
{
    public static class WorldRegistry
    {
        private static readonly List<WorldObject> WorldObjects = new List<WorldObject>();
        public static List<NomaiOrbTransformSync> OrbSyncList = new List<NomaiOrbTransformSync>();
        public static List<NomaiInterfaceOrb> OldOrbList = new List<NomaiInterfaceOrb>();

        public static void AddObject(WorldObject worldObject)
        {
            if (WorldObjects.Contains(worldObject))
            {
                return;
            }
            WorldObjects.Add(worldObject);
        }

        public static IEnumerable<T> GetObjects<T>()
        {
            return WorldObjects.OfType<T>();
        }

        public static T GetObject<T>(int id) where T : WorldObject
        {
            return GetObjects<T>().FirstOrDefault(x => x.ObjectId == id);
        }

        public static void RemoveObjects<T>() where T : WorldObject
        {
            WorldObjects.RemoveAll(x => x.GetType() == typeof(T));
        }

        public static void HandleSlotStateChange(NomaiInterfaceSlot slot, NomaiInterfaceOrb affectingOrb, bool state)
        {
            var slotList = GetObjects<QSBOrbSlot>();
            if (slotList.Count() == 0)
            {
                DebugLog.ToConsole($"Error - No QSBOrbSlots found when handling slot state change.", MessageType.Error);
                return;
            }
            var qsbSlot = slotList.First(x => x.InterfaceSlot == slot);
            var orbSync = OrbSyncList.First(x => x.AttachedOrb == affectingOrb);
            if (orbSync.hasAuthority)
            {
                qsbSlot.HandleEvent(state);
            }
        }

        public static void RaiseEvent(object instance, string eventName)
        {
            if (!(instance.GetType()
                .GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(instance) is MulticastDelegate multiDelegate))
            {
                return;
            }
            var delegateList = multiDelegate.GetInvocationList().ToList();
            delegateList.ForEach(x => x.DynamicInvoke(instance));
        }
    }
}
