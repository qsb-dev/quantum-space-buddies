using System.Collections.Generic;
using QSB.Messaging;
using UnityEngine;

namespace QSB.TransformSync
{
    public class SectorSync : MonoBehaviour
    {
        public static SectorSync Instance { get; private set; }

        private Dictionary<uint, Transform> _playerSectors;
        private Sector[] _allSectors;
        private MessageHandler<SectorMessage> _sectorHandler;
        private readonly List<Sector.Name> SectorWhitelist = new List<Sector.Name>(new Sector.Name[]{
            Sector.Name.BrambleDimension,
            Sector.Name.BrittleHollow,
            Sector.Name.Comet,
            Sector.Name.DarkBramble,
            Sector.Name.EyeOfTheUniverse,
            Sector.Name.GiantsDeep,
            Sector.Name.HourglassTwin_A,
            Sector.Name.HourglassTwin_B,
            Sector.Name.OrbitalProbeCannon,
            Sector.Name.QuantumMoon,
            Sector.Name.SunStation,
            Sector.Name.TimberHearth,
            Sector.Name.TimberMoon,
            Sector.Name.VolcanicMoon,
            Sector.Name.WhiteHole
        });

        private void Start()
        {
            Instance = this;
            DebugLog.Screen("Start SectorSync");
            _playerSectors = new Dictionary<uint, Transform>();

            _sectorHandler = new MessageHandler<SectorMessage>();
            _sectorHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _sectorHandler.OnServerReceiveMessage += OnServerReceiveMessage;

            QSB.Helper.HarmonyHelper.AddPrefix<Sector>("OnEntry", typeof(Patches), nameof(Patches.PreEntry));
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
            public static void PreEntry(Sector __instance, GameObject hitObj)
            {
                SectorDetector sectorDetector = hitObj.GetComponent<SectorDetector>();
                if (sectorDetector == null)
                {
                    return;
                }

                if (!Instance.SectorWhitelist.Contains(__instance.GetName()))
                {
                    return;
                }

                if (sectorDetector.GetOccupantType() == DynamicOccupant.Player && PlayerTransformSync.LocalInstance != null)
                {
                    PlayerTransformSync.LocalInstance.EnterSector(__instance);
                    return;
                }

                if (sectorDetector.GetOccupantType() == DynamicOccupant.Ship && ShipTransformSync.LocalInstance != null)
                {
                    ShipTransformSync.LocalInstance.EnterSector(__instance);
                }
            }
        }

    }
}
