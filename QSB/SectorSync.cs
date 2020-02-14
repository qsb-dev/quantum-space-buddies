using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class SectorSync: MessageHandler {
        protected override short type => MessageType.Sector;

        static Dictionary<uint, Transform> playerSectors;
        static Sector[] _allSectors = null;

        void Start () {
            DebugLog.Screen("Start SectorSync");
            playerSectors = new Dictionary<uint, Transform>();

            QSB.Helper.HarmonyHelper.AddPrefix<PlayerSectorDetector>("OnAddSector", typeof(Patches), "PreAddSector");
        }

        public static void SetSector (uint id, Transform sectorTransform) {
            playerSectors[id] = sectorTransform;
        }

        public static void SetSector (uint id, Sector.Name sectorName) {
            if (sectorName == Sector.Name.Unnamed || sectorName == Sector.Name.Ship && sectorName == Sector.Name.Sun) {
                return;
            }

            DebugLog.Screen("Gonna set sector");

            playerSectors[id] = FindSectorTransform(sectorName);

            SectorMessage msg = new SectorMessage();
            msg.sectorId = (int) sectorName;
            msg.senderId = id;
            NetworkManager.singleton.client.Send(MessageType.Sector, msg);

        }

        public static Transform GetSector (uint id) {
            return playerSectors[id];
        }

        static Transform FindSectorTransform (Sector.Name sectorName) {
            if (_allSectors == null) {
                _allSectors = FindObjectsOfType<Sector>();
            }
            foreach (var sector in _allSectors) {
                if (sectorName == sector.GetName()) {
                    return sector.transform;
                }
            }
            return null;
        }

        protected override void OnClientReceiveMessage (NetworkMessage netMsg) {
            DebugLog.Screen("OnClientReceiveMessage SectorSync");
            SectorMessage msg = netMsg.ReadMessage<SectorMessage>();

            var sectorName = (Sector.Name) msg.sectorId;
            var sectorTransform = FindSectorTransform(sectorName);

            if (sectorTransform == null) {
                DebugLog.Screen("Sector", sectorName, "not found");
                return;
            }

            DebugLog.Screen("Found sector", sectorName, ", setting for", msg.senderId);
            playerSectors[msg.senderId] = sectorTransform;
        }

        protected override void OnServerReceiveMessage (NetworkMessage netMsg) {
            DebugLog.Screen("OnServerReceiveMessage SectorSync");
            SectorMessage msg = netMsg.ReadMessage<SectorMessage>();
            NetworkServer.SendToAll(MessageType.Sector, msg);
        }

        static class Patches {
            static void PreAddSector (Sector sector, PlayerSectorDetector __instance) {
                if (NetworkPlayer.localInstance != null) {
                    NetworkPlayer.localInstance.EnterSector(sector);
                }
            }
        }
    }
}
