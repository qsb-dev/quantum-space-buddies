using System.Collections.Generic;
using OWML.Common;
using QSB.AnglerFish.WorldObjects;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.AnglerFish {
    public class AnglerManager : WorldObjectManager {
        /// holds sectors and their occupants
        private static Dictionary<string, HashSet<uint>> sectorOccupants;

        public static HashSet<uint> GetSectorOccupants(string sector) {
            if (!QSBCore.IsHost)
                DebugLog.ToConsole("Error - getting angler sector occupants as non-host", MessageType.Error);

            if (sectorOccupants.TryGetValue(sector, out var occupants)) return occupants;

            occupants = new HashSet<uint>();
            sectorOccupants[sector] = occupants;
            return occupants;
        }

        protected override void RebuildWorldObjects(OWScene scene) {
            QSBWorldSync.Init<QSBAngler, AnglerfishController>();

            if (QSBCore.IsHost) sectorOccupants = new Dictionary<string, HashSet<uint>>();
        }

        public override void Awake() {
            base.Awake();
            QSBPlayerManager.OnRemovePlayer += OnRemovePlayer;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            QSBPlayerManager.OnRemovePlayer -= OnRemovePlayer;
        }

        private static void OnRemovePlayer(uint id) {
            if (!QSBCore.IsHost) return;
            foreach (var occupants in sectorOccupants.Values)
                occupants.Remove(id);
        }
    }
}
