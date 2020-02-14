using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class SectorSync: MessageHandler {
        protected override short type => MessageType.Sector;
        static Dictionary<uint, Transform> playerSectors;
        static Sector[] _allSectors = null;

        void Start () {
            playerSectors = new Dictionary<uint, Transform>();

            QSB.Helper.HarmonyHelper.AddPrefix<PlayerSectorDetector>("OnAddSector", typeof(Patches), "PreAddSector");
        }

        public static void SetSector (NetworkInstanceId netId, Sector.Name sectorName, bool skipAnnounce = false) {
            if (sectorName == Sector.Name.Unnamed || sectorName == Sector.Name.Ship && sectorName == Sector.Name.Sun) {
                return;
            }

            playerSectors[netId.Value] = FindSectorTransform(sectorName);

            if (!skipAnnounce) {
                SectorMessage msg = new SectorMessage();
                msg.sectorId = (int) sectorName;
                msg.senderId = netId.Value;
                NetworkManager.singleton.client.Send(MessageType.Sector, msg);
            }
        }

        public static Transform GetSector (NetworkInstanceId netId) {
            return playerSectors[netId.Value];
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
