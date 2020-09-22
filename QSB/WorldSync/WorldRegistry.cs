using QSB.OrbSync;
using QSB.TransformSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.WorldSync
{
    public static class WorldRegistry
    {
        private static readonly List<WorldObject> WorldObjects = new List<WorldObject>();
        public static List<NomaiOrbTransformSync> OrbSyncList = new List<NomaiOrbTransformSync>();
        public static List<NomaiInterfaceOrb> OldOrbList = new List<NomaiInterfaceOrb>();
        public static List<CharacterDialogueTree> OldDialogueTrees = new List<CharacterDialogueTree>();

        public static void InitOnSceneLoaded(OWScene scene, bool inUniverse)
        {
            OldOrbList = Resources.FindObjectsOfTypeAll<NomaiInterfaceOrb>().ToList();
            if (NetworkServer.active)
            {
                OldOrbList.ForEach(x => NetworkServer.Spawn(UnityEngine.Object.Instantiate(QSBNetworkManager.Instance.OrbPrefab)));
            }

            OldDialogueTrees = Resources.FindObjectsOfTypeAll<CharacterDialogueTree>().ToList();
        }

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
            var qsbSlot = GetObjects<QSBOrbSlot>().First(x => x.InterfaceSlot == slot);
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
