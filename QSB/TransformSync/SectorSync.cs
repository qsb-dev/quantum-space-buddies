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
        private MessageHandler<SectorMessage> _sectorHandler;

        private Sector[] _allSectors;

        private void Start()
        {
            Instance = this;

            _sectorHandler = new MessageHandler<SectorMessage>();
            _sectorHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _sectorHandler.OnServerReceiveMessage += OnServerReceiveMessage;

            QSB.Helper.Events.Scenes.OnCompleteSceneChange += OnCompleteSceneChange;
        }

        private void OnCompleteSceneChange(OWScene oldScene, OWScene newScene)
        {
            _allSectors = FindObjectsOfType<Sector>();
        }

        private void SendSector(uint id, Sector.Name sectorName)
        {
            DebugLog.ToScreen($"Sending sector {sectorName} for {PlayerRegistry.GetPlayer(id).Name}");

            var msg = new SectorMessage
            {
                SectorId = (int)sectorName,
                SenderId = id
            };
            _sectorHandler.SendToServer(msg);
        }

        private Transform FindSectorTransform(Sector.Name sectorName)
        {
            return _allSectors?
                .Where(sector => sectorName == sector.GetName())
                .Select(sector => sector.transform)
                .FirstOrDefault();
        }

        private void OnClientReceiveMessage(SectorMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            if (player == PlayerRegistry.LocalPlayer)
            {
                return;
            }

            var sectorName = (Sector.Name)message.SectorId;

            DebugLog.ToScreen($"Received sector {sectorName} for {player.Name}");

            var sectorTransform = FindSectorTransform(sectorName);

            if (sectorTransform == null)
            {
                DebugLog.ToScreen($"Could not find transform for {sectorName}");
                return;
            }

            player.ReferenceSector = sectorTransform;
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
            var sector = GetClosestSector(me);

            var sectorTransform = FindSectorTransform(sector);

            if (sectorTransform == null)
            {
                DebugLog.ToAll("ERROR! Sector transform not found for sector " + sector);
                return;
            }

            me.ReferenceSector = sectorTransform;

            SendSector(me.NetId, sector);
            _sendTimer = 0;
        }

        private Sector.Name GetClosestSector(PlayerInfo player)
        {
            return _allSectors
                .OrderBy(s => Vector3.Distance(s.transform.position, player.Position))
                .Select(s => s.GetName())
                .Except(new[] { Sector.Name.Unnamed, Sector.Name.Ship })
                .First();
        }
    }
}
