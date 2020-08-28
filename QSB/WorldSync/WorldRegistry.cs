using QSB.TransformSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.WorldSync
{
    public static class WorldRegistry
    {
        private static readonly List<WorldObject> _worldObjects = new List<WorldObject>();
        public static uint OrbStartNetId { get; set; }
        public static List<NomaiOrbTransformSync> OrbList = new List<NomaiOrbTransformSync>();
        public static List<NomaiInterfaceOrb> OldOrbList = new List<NomaiInterfaceOrb>();
        public static Dictionary<NomaiInterfaceOrb, uint> OrbUserList = new Dictionary<NomaiInterfaceOrb, uint>();

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

        public static bool IsOrbControlledLocally(NomaiInterfaceOrb orb)
        {
            return OrbUserList[orb] == PlayerRegistry.LocalPlayerId;
        }
    }
}
