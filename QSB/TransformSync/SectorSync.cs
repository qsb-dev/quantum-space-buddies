using QSB.Messaging;
using UnityEngine;
using System.Linq;
using QSB.Utility;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public class SectorSync : NetworkBehaviour
    {
        private Sector[] _allSectors;
        private Sector _lastSector;
        private MessageHandler<SectorMessage> _sectorHandler;

        private readonly Sector.Name[] _sectorBlacklist = {
            Sector.Name.Unnamed,
            Sector.Name.Ship
        };

        private void Start()
        {
            if (!isLocalPlayer)
            {
                return;
            }
            _sectorHandler = new MessageHandler<SectorMessage>();
            _sectorHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _sectorHandler.OnServerReceiveMessage += OnServerReceiveMessage;

            QSB.Helper.Events.Scenes.OnCompleteSceneChange += OnCompleteSceneChange;
        }

        private void OnCompleteSceneChange(OWScene oldScene, OWScene newScene)
        {
            _allSectors = FindObjectsOfType<Sector>();
        }

        private void SetSector(Sector sector)
        {
            var me = PlayerRegistry.LocalPlayer;
            me.ReferenceSector = sector.transform;

            DebugLog.ToScreen($"Sending my ({me.Name}) reference object {sector.GetName()}");

            var msg = new SectorMessage
            {
                SectorId = (int)sector.GetName(),
                SenderId = me.NetId
            };
            _sectorHandler.SendToServer(msg);
        }

        private Sector FindSector(Sector.Name sectorName)
        {
            return _allSectors?
                .FirstOrDefault(sector => sectorName == sector.GetName());
        }

        private void OnClientReceiveMessage(SectorMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            if (player == PlayerRegistry.LocalPlayer)
            {
                return;
            }

            DebugLog.ToScreen($"Received sector {message.SectorName} for {player.Name}");

            player.ReferenceSector = FindSector(message.SectorName)?.transform;
        }

        private void OnServerReceiveMessage(SectorMessage message)
        {
            _sectorHandler.SendToAll(message);
        }

        private void Update()
        {
            if (!isLocalPlayer || _allSectors == null || _allSectors.Length == 0)
            {
                return;
            }

            var sector = GetClosestSector(PlayerRegistry.LocalPlayer);
            if (sector == _lastSector)
            {
                return;
            }

            SetSector(sector);
            _lastSector = sector;
        }

        private Sector GetClosestSector(PlayerInfo player)
        {
            return _allSectors
                .Where(sector => !_sectorBlacklist.Contains(sector.GetName()))
                .OrderBy(sector => Vector3.Distance(sector.transform.position, player.Position))
                .First();
        }
    }
}
