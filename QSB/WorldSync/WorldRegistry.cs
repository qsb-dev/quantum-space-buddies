using System.Collections.Generic;
using System.Linq;

namespace QSB.WorldSync
{
    public static class WorldRegistry
    {
        public static List<WorldObject> WorldObjects { get; } = new List<WorldObject>();

        public static T GetObject<T>(int id) where T : WorldObject
        {
            return WorldObjects.OfType<T>().FirstOrDefault(x => x.Id == id);
        }
    }
}
