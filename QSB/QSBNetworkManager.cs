using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    class QSBNetworkManager: NetworkManager {
        public static Dictionary<uint, Transform> playerSectors;

        void Awake () {
            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/network");
            playerPrefab = assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            playerPrefab.AddComponent<NetworkPlayer>();

            playerSectors = new Dictionary<uint, Transform>();
        }

        public override void OnStartClient (NetworkClient client) {
            base.OnStartClient(client);

            NetworkServer.RegisterHandler(SectorMessage.Type, OnReceiveMessage);
            client.RegisterHandler(SectorMessage.Type, OnReceiveMessage);
        }

        public static Transform GetSectorByName (Sector.Name sectorName) {
            var sectors = GameObject.FindObjectsOfType<Sector>();
            foreach (var sector in sectors) {
                if (sectorName == sector.GetName()) {
                    return sector.transform;
                }
            }
            return null;
        }

        public static void OnReceiveMessage (NetworkMessage netMsg) {
            QSB.LogToScreen("Global message receive");
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
