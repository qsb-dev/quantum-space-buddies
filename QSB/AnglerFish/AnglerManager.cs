using System.Collections.Generic;
using QSB.AnglerFish.WorldObjects;
using QSB.WorldSync;

namespace QSB.AnglerFish {
    public class AnglerManager : WorldObjectManager {
        /// holds which player is in which sector
        public static Dictionary<uint, string> sectorOccupants;

        protected override void RebuildWorldObjects(OWScene scene) {
            QSBWorldSync.Init<QSBAngler, AnglerfishController>();

            if (QSBCore.IsHost) sectorOccupants = new Dictionary<uint, string>();
        }
    }
}
