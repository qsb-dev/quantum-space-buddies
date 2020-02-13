using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class SectorSync: MessageHandler {
        public override short type { get => MessageType.Sector; }
        public static Dictionary<uint, Transform> playerSectors;

        void Awake () {
            playerSectors = new Dictionary<uint, Transform>();
        }

        public static Transform GetSectorByName (Sector.Name sectorName) {
            var sectors = FindObjectsOfType<Sector>();
            foreach (var sector in sectors) {
                if (sectorName == sector.GetName()) {
                    return sector.transform;
                }
            }
            return null;
        }

        protected override void OnReceiveMessage (NetworkMessage netMsg) {
            SectorMessage msg = netMsg.ReadMessage<SectorMessage>();

            var sectorName = (Sector.Name) msg.sectorId;
            var sectorTransform = GetSectorByName(sectorName);

            if (sectorTransform == null) {
                QSB.LogToScreen("Sector", sectorName, "not found");
                return;
            }

            QSB.LogToScreen("Found sector", sectorName, ", setting for", msg.senderId);
            playerSectors[msg.senderId] = sectorTransform;
        }
    }
}
