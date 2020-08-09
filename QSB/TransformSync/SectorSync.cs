using System.Collections.Generic;
using QSB.Messaging;
using UnityEngine;
using System.Linq;
using QSB.Utility;

namespace QSB.TransformSync
{
    public class SectorSync : MonoBehaviour
    {
        private readonly List<Sector> _allSectors = new List<Sector>();
        private MessageHandler<SectorMessage> _sectorHandler;

        private readonly Sector.Name[] _sectorBlacklist =
        {
            Sector.Name.Unnamed,
            Sector.Name.Ship
        };

        private void Awake()
        {
            QSB.Helper.Events.Subscribe<Sector>(OWML.Common.Events.AfterAwake);
            QSB.Helper.Events.Event += OnEvent;
        }

        private void OnEvent(MonoBehaviour behaviour, OWML.Common.Events ev)
        {
            if (behaviour is Sector sector && ev == OWML.Common.Events.AfterAwake)
            {
                if (!_allSectors.Contains(sector))
                {
                    _allSectors.Add(sector);
                }
            }
        }

        private void Start()
        {
            _sectorHandler = new MessageHandler<SectorMessage>();
            _sectorHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _sectorHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        private void SendSector(uint id, Sector sector)
        {
            DebugLog.ToScreen($"Sending sector {sector.name} for id {id}");

            var msg = new SectorMessage
            {
                SectorId = (int)sector.GetName(),
                SectorName = sector.name,
                SenderId = id
            };
            _sectorHandler.SendToServer(msg);
        }

        private Sector FindSectorByName(Sector.Name sectorName, string goName)
        {
            return _allSectors
                .FirstOrDefault(sector => sector != null &&
                                          sector.GetName() == sectorName &&
                                          sector.name == goName);
        }

        private void OnClientReceiveMessage(SectorMessage message)
        {
            var sector = FindSectorByName((Sector.Name)message.SectorId, message.SectorName);

            if (_allSectors.Count == 0)
            {
                DebugLog.ToConsole($"Error: _allSectors is empty for player {message.SenderId}!", OWML.Common.MessageType.Error);
            }

            if (sector == null)
            {
                DebugLog.ToScreen($"Sector {message.SectorName},{(Sector.Name)message.SectorId} not found!");
                return;
            }

            var transformSync = PlayerRegistry.GetTransformSync(message.SenderId);
            DebugLog.ToScreen($"{transformSync.GetType().Name} of ID {message.SenderId} set to {message.SectorName}");
            transformSync.ReferenceTransform = sector.transform;
        }

        private void OnServerReceiveMessage(SectorMessage message)
        {
            _sectorHandler.SendToAll(message);
        }

        private void Update()
        {
            if (_allSectors == null || !_allSectors.Any())
            {
                return;
            }
            PlayerRegistry.LocalTransformSyncs.ForEach(UpdateTransformSync);
        }

        private void UpdateTransformSync(TransformSync transformSync)
        {
            var syncedTransform = transformSync.SyncedTransform;
            if (syncedTransform == null ||
                syncedTransform.position == Vector3.zero ||
                syncedTransform.position == Locator.GetAstroObject(AstroObject.Name.Sun).transform.position)
            {
                return;
            }
            var closestSector = GetClosestSector(syncedTransform);
            if (closestSector.transform == transformSync.ReferenceTransform)
            {
                return;
            }
            SendSector(transformSync.netId.Value, closestSector);
            transformSync.ReferenceTransform = closestSector.transform;
        }

        private Sector GetClosestSector(Transform trans)
        {
            return _allSectors
                .Where(sector => sector != null &&
                                 !_sectorBlacklist.Contains(sector.GetName()))
                .OrderBy(sector => Vector3.Distance(sector.transform.position, trans.position))
                .First();
        }
    }
}
