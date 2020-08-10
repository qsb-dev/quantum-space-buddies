using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QSB.Events;
using QSB.Utility;

namespace QSB.TransformSync
{
    public class SectorSync : MonoBehaviour
    {
        private readonly List<Sector> _allSectors = new List<Sector>();

        public static SectorSync LocalInstance { get; private set; }

        private readonly Sector.Name[] _sectorBlacklist =
        {
            Sector.Name.Unnamed,
            Sector.Name.Ship
        };

        private void Awake()
        {
            LocalInstance = this;
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

        private void SendSector(uint id, Sector sector)
        {
            DebugLog.ToScreen($"Sending sector {sector.name} for id {id}");
            GlobalMessenger<uint, int, string>.FireEvent(EventNames.QSBSectorChange, id, (int)sector.GetName(), sector.name);
        }

        public Sector FindSectorByName(Sector.Name sectorName, string goName)
        {
            if (_allSectors.Count == 0)
            {
                DebugLog.ToConsole("Error: _allSectors is empty!", OWML.Common.MessageType.Error);
            }

            return _allSectors
                .FirstOrDefault(sector => sector != null &&
                                          sector.GetName() == sectorName &&
                                          sector.name == goName);
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
