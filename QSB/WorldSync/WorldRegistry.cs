using QSB.OrbSync;
using QSB.TransformSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QSB.WorldSync
{
    public static class WorldRegistry
    {
        private static readonly List<WorldObject> _worldObjects = new List<WorldObject>();
        public static List<NomaiOrbTransformSync> OrbSyncList = new List<NomaiOrbTransformSync>();
        public static List<NomaiInterfaceOrb> OldOrbList = new List<NomaiInterfaceOrb>();

        public static void AddObject(WorldObject worldObject)
        {
            if (_worldObjects.Contains(worldObject))
            {
                return;
            }
            _worldObjects.Add(worldObject);
        }

        public static IEnumerable<T> GetObjects<T>()
        {
            return _worldObjects.OfType<T>();
        }

        public static T GetObject<T>(int id) where T : WorldObject
        {
            return GetObjects<T>().FirstOrDefault(x => x.ObjectId == id);
        }

        public static void HandleSlotStateChange(NomaiInterfaceSlot slot, NomaiInterfaceOrb affectingOrb, bool state)
        {
            var qsbSlot = GetObjects<QSBOrbSlot>().First(x => x.InterfaceSlot == slot);
            var orbSync = OrbSyncList.First(x => x.AttachedOrb == affectingOrb);
            if (orbSync.hasAuthority)
            {
                qsbSlot.HandleEvent(state);
            }
        }

        public static void RaiseEvent(object instance, string eventName)
        {
            var type = instance.GetType();
            var staticFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var fieldInfo = type.GetField(eventName, staticFlags);
            var multDelegate = fieldInfo.GetValue(instance) as MulticastDelegate;
            if (multDelegate == null)
            {
                return;
            }
            var delegateList = multDelegate.GetInvocationList().ToList();
            delegateList.ForEach(x => x.DynamicInvoke(instance));
        }
    }
}
