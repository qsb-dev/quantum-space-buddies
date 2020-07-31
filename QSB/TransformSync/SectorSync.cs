using QSB.Messaging;
using UnityEngine;
using System.Linq;
using QSB.Utility;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public class SectorSync : NetworkBehaviour
    {
        public static SectorSync Instance { get; private set; }

        private const float SendInterval = 0.5f;
        private float _sendTimer;
        private Sector.Name _lastSentSector;
        private MessageHandler<SectorMessage> _sectorHandler;

        private Sector[] _allSectors;
        private Sector[] AllSectors
        {
            get
            {
                if (_allSectors == null || !_allSectors.Any())
                {
                    _allSectors = FindObjectsOfType<Sector>();
                }
                return _allSectors;
            }
        }
        
        private void Start()
        {
            Instance = this;

            _sectorHandler = new MessageHandler<SectorMessage>();
            _sectorHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _sectorHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        private void SendSector(uint id, Sector.Name sectorName)
        {

            if (_lastSentSector == sectorName)
            {
                return;
            }

            DebugLog.ToScreen("Gonna send sector");

            PlayerRegistry.GetPlayer(id).ReferenceSector = FindSectorTransform(sectorName);

            var msg = new SectorMessage
            {
                SectorId = (int)sectorName,
                SenderId = id
            };
            _sectorHandler.SendToServer(msg);
            _lastSentSector = sectorName;
        }

        private Transform FindSectorTransform(Sector.Name sectorName)
        {
            return AllSectors
                .Where(sector => sectorName == sector.GetName())
                .Select(sector => sector.transform)
                .FirstOrDefault();
        }

        private void OnClientReceiveMessage(SectorMessage message)
        {
            DebugLog.ToScreen("OnClientReceiveMessage SectorSync");

            var sectorName = (Sector.Name)message.SectorId;
            var sectorTransform = FindSectorTransform(sectorName);

            if (sectorTransform == null)
            {
                DebugLog.ToScreen("Sector", sectorName, "not found");
                return;
            }

            DebugLog.ToScreen("Found sector", sectorName, ", setting for", message.SenderId);
            PlayerRegistry.GetPlayer(message.SenderId).ReferenceSector = sectorTransform;
        }

        private void OnServerReceiveMessage(SectorMessage message)
        {
            DebugLog.ToScreen("OnServerReceiveMessage SectorSync");
            _sectorHandler.SendToAll(message);
        }

        private void Update()
        {
            if (!isLocalPlayer || _allSectors == null || _allSectors.Length == 0)
            {
                return;
            }
            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer < SendInterval)
            {
                return;
            }
            var me = PlayerRegistry.LocalPlayer;
            var sector = AllSectors.OrderByDescending(s => Vector3.Distance(s.transform.position, me.Position)).First();
            SendSector(me.NetId, sector.GetName());
            _sendTimer = 0;
        }

    }
}
