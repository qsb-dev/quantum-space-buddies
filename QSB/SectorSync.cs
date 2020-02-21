using QSB.Messaging;
using System.Collections.Generic;
using UnityEngine;

namespace QSB
{
    public class SectorSync : MonoBehaviour
    {
        public static SectorSync Instance { get; private set; }

        private Dictionary<uint, Transform> _playerSectors;
        private Sector[] _allSectors;
        private MessageHandler<SectorMessage> _sectorHandler;

        private void Start()
        {
            Instance = this;
            DebugLog.Screen("Start SectorSync");
            _playerSectors = new Dictionary<uint, Transform>();

            _sectorHandler = new MessageHandler<SectorMessage>();
            _sectorHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _sectorHandler.OnServerReceiveMessage += OnServerReceiveMessage;

            QSB.Helper.HarmonyHelper.AddPrefix<SectorDetector>("AddSector", typeof(Patches), "PreAddSector");
        }

        public void SetSector(uint id, Transform sectorTransform)
        {
            _playerSectors[id] = sectorTransform;
        }

        public void SetSector(uint id, Sector.Name sectorName)
        {
            DebugLog.Screen("Gonna set sector");

            _playerSectors[id] = FindSectorTransform(sectorName);

            var msg = new SectorMessage
            {
                SectorId = (int)sectorName,
                SenderId = id
            };
            _sectorHandler.SendToServer(msg);
        }

        public Transform GetSector(uint id)
        {
            return _playerSectors[id];
        }

        private Transform FindSectorTransform(Sector.Name sectorName)
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

        private void OnClientReceiveMessage(SectorMessage message)
        {
            DebugLog.Screen("OnClientReceiveMessage SectorSync");

            var sectorName = (Sector.Name)message.SectorId;
            var sectorTransform = FindSectorTransform(sectorName);

            if (sectorTransform == null)
            {
                DebugLog.Screen("Sector", sectorName, "not found");
                return;
            }

            DebugLog.Screen("Found sector", sectorName, ", setting for", message.SenderId);
            _playerSectors[message.SenderId] = sectorTransform;
        }

        private void OnServerReceiveMessage(SectorMessage message)
        {
            DebugLog.Screen("OnServerReceiveMessage SectorSync");
            _sectorHandler.SendToAll(message);
        }

        private static class Patches
        {
            private static void PreAddSector(Sector sector, DynamicOccupant ____occupantType)
            {
                if (sector.GetName() == Sector.Name.Unnamed || sector.GetName() == Sector.Name.Ship || sector.GetName() == Sector.Name.Sun || sector.GetName() == Sector.Name.HourglassTwins)
                {
                    return;
                }

                if (____occupantType == DynamicOccupant.Player && NetworkPlayer.LocalInstance != null)
                {
                    NetworkPlayer.LocalInstance.EnterSector(sector);
                    return;
                }

                if (____occupantType == DynamicOccupant.Ship && ShipTransformSync.LocalInstance != null)
                {
                    ShipTransformSync.LocalInstance.EnterSector(sector);
                }
            }
        }

    }
}
