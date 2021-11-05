using System.Collections.Generic;
using QSB.AnglerFish.WorldObjects;
using QSB.WorldSync;

namespace QSB.AnglerFish {
    public class AnglerManager : WorldObjectManager {
        /// holds sectors and their occupants
        public static Dictionary<string, HashSet<uint>> sectorOccupants;

        protected override void RebuildWorldObjects(OWScene scene) {
            QSBWorldSync.Init<QSBAngler, AnglerfishController>();

            if (QSBCore.IsHost) sectorOccupants = new Dictionary<string, HashSet<uint>>();
        }
    }
}
