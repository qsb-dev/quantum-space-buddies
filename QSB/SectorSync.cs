using QSB.Messaging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class SectorSync : MessageHandler
    {
        protected override MessageType Type => MessageType.Sector;

        private static Dictionary<uint, Transform> _playerSectors;
        private static Sector[] _allSectors;

        private void Start()
        {
            DebugLog.Screen("Start SectorSync");
            _playerSectors = new Dictionary<uint, Transform>();

            QSB.Helper.HarmonyHelper.AddPrefix<PlayerSectorDetector>("OnAddSector", typeof(Patches), "PreAddSector");
        }

        public static void SetSector(uint id, Transform sectorTransform)
        {
            _playerSectors[id] = sectorTransform;
        }

        public static void SetSector(uint id, Sector.Name sectorName)
        {
            DebugLog.Screen("Gonna set sector");

            _playerSectors[id] = FindSectorTransform(sectorName);

            var msg = new SectorMessage
            {
                SectorId = (int)sectorName,
                SenderId = id
            };
            NetworkManager.singleton.client.Send((short)MessageType.Sector, msg);

        }

        public static Transform GetSector(uint id)
        {
            return _playerSectors[id];
        }

        private static Transform FindSectorTransform(Sector.Name sectorName)
        {
            if (_allSectors == null)
            {
                _allSectors = FindObjectsOfType<Sector>();
            }
            foreach (var sector in _allSectors)
            {
                if (sectorName == sector.GetName())
                {
                    return sector.transform;
                }
            }
            return null;
        }

        protected override void OnClientReceiveMessage(NetworkMessage netMsg)
        {
            DebugLog.Screen("OnClientReceiveMessage SectorSync");
            var msg = netMsg.ReadMessage<SectorMessage>();

            var sectorName = (Sector.Name)msg.SectorId;
            var sectorTransform = FindSectorTransform(sectorName);

            if (sectorTransform == null)
            {
                DebugLog.Screen("Sector", sectorName, "not found");
                return;
            }

            DebugLog.Screen("Found sector", sectorName, ", setting for", msg.SenderId);
            _playerSectors[msg.SenderId] = sectorTransform;
        }

        protected override void OnServerReceiveMessage(NetworkMessage netMsg)
        {
            DebugLog.Screen("OnServerReceiveMessage SectorSync");
            var msg = netMsg.ReadMessage<SectorMessage>();
            NetworkServer.SendToAll((short)MessageType.Sector, msg);
        }

        private static class Patches
        {
            private static void PreAddSector(Sector sector, PlayerSectorDetector __instance)
            {
                if (sector.GetName() == Sector.Name.Unnamed || sector.GetName() == Sector.Name.Ship || sector.GetName() == Sector.Name.Sun || sector.GetName() == Sector.Name.HourglassTwins)
                {
                    return;
                }

                if (NetworkPlayer.LocalInstance != null)
                {
                    NetworkPlayer.LocalInstance.EnterSector(sector);
                }
            }
        }

    }
}
