using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
    public class QSBSectorManager : MonoBehaviour
    {
        public static QSBSectorManager Instance { get; private set; }

        public bool IsReady { get; private set; }

        private readonly Sector.Name[] _sectorBlacklist =
        {
            Sector.Name.Ship
        };

        private void Awake()
        {
            Instance = this;
            QSBSceneManager.OnSceneLoaded += (OWScene scene, bool universe) => RebuildSectors();
            DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
        }

        public void RebuildSectors()
        {
            DebugLog.DebugWrite("Rebuilding sectors...", MessageType.Warning);
            WorldRegistry.RemoveObjects<QSBSector>();
            var sectors = Resources.FindObjectsOfTypeAll<Sector>().ToList();
            for (var id = 0; id < sectors.Count; id++)
            {
                var qsbSector = WorldRegistry.GetObject<QSBSector>(id) ?? new QSBSector();
                qsbSector.Init(sectors[id], id);
                WorldRegistry.AddObject(qsbSector);
            }
            IsReady = WorldRegistry.GetObjects<QSBSector>().Any();
        }

        public QSBSector GetClosestSector(Transform trans)
        {
            return WorldRegistry.GetObjects<QSBSector>()
                .Where(sector => sector.Sector != null && !_sectorBlacklist.Contains(sector.Type))
                .OrderBy(sector => Vector3.Distance(sector.Position, trans.position))
                .First();
        }

        public QSBSector GetStartPlanetSector()
        {
            var sector = QSBSceneManager.CurrentScene == OWScene.SolarSystem
                ? Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetRootSector()
                : Locator.GetAstroObject(AstroObject.Name.Eye).GetRootSector();
            return WorldRegistry.GetObjects<QSBSector>()
                .FirstOrDefault(x => x.Sector == sector);
        }
    }
}
