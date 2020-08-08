using QSB.Messaging;
using UnityEngine;
using System.Linq;
using QSB.Utility;
using UnityEngine.SceneManagement;

namespace QSB.TransformSync
{
    public class SectorSync : MonoBehaviour
    {
        private Sector[] _allSectors;
        private MessageHandler<SectorMessage> _sectorHandler;

        private readonly Sector.Name[] _sectorBlacklist =
        {
            Sector.Name.Unnamed,
            Sector.Name.Ship
        };

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            _sectorHandler = new MessageHandler<SectorMessage>();
            _sectorHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _sectorHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _allSectors = FindObjectsOfType<Sector>();
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
            return _allSectors?
                .FirstOrDefault(sector => sector.GetName() == sectorName && sector.name == goName);
        }

        private void OnClientReceiveMessage(SectorMessage message)
        {
            DebugLog.ToScreen($"Received sector {message.SectorName} for id {message.SenderId}");

            var sector = FindSectorByName((Sector.Name)message.SectorId, message.SectorName);

            if (sector == null)
            {
                DebugLog.ToScreen($"Sector {message.SectorName} not found");
                return;
            }

            DebugLog.ToScreen($"Found sector {message.SectorName} for {message.SenderId}");
            PlayerRegistry.GetTransformSync(message.SenderId).ReferenceTransform = sector.transform;
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
            return _allSectors?
                .Where(sector => !_sectorBlacklist.Contains(sector.GetName()))
                .OrderBy(sector => Vector3.Distance(sector.transform.position, trans.position))
                .First();
        }
    }
}
