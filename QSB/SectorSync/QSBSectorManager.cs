using OWML.Common;
using QSB.SectorSync.WorldObjects;
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

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += (OWScene scene) => RebuildSectors();
			QSBSceneManager.OnUniverseSceneLoaded += (OWScene scene) => QSBCore.Helper.Events.Unity.RunWhen(() => Locator.GetPlayerSectorDetector() != null, StartThing);
			DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
		}

		public void OnDestroy() => QSBSceneManager.OnUniverseSceneLoaded -= (OWScene scene) => RebuildSectors();

		private void StartThing()
		{
			Locator.GetPlayerSectorDetector().OnEnterSector += (Sector sector) => DebugLog.DebugWrite($"Player enter sector {sector.name}", MessageType.Success);
			Locator.GetPlayerSectorDetector().OnExitSector += (Sector sector) => DebugLog.DebugWrite($"Player exit sector {sector.name}", MessageType.Warning);
		}

		public void RebuildSectors()
		{
			DebugLog.DebugWrite("Rebuilding sectors...", MessageType.Warning);
			QSBWorldSync.RemoveWorldObjects<QSBSector>();
			QSBWorldSync.Init<QSBSector, Sector>();
			IsReady = QSBWorldSync.GetWorldObjects<QSBSector>().Any();
		}

		public QSBSector GetClosestSector(Transform trans) // trans rights \o/
		{
			if (QSBWorldSync.GetWorldObjects<QSBSector>().Count() == 0)
			{
				DebugLog.DebugWrite($"Error - Can't get closest sector, as there are no QSBSectors!", MessageType.Error);
				return null;
			}
			return QSBWorldSync.GetWorldObjects<QSBSector>()
				.Where(sector => sector.AttachedObject != null
					&& !_sectorBlacklist.Contains(sector.Type)
					&& sector.Transform.gameObject.activeInHierarchy)
				.OrderBy(sector => Vector3.Distance(sector.Position, trans.position))
				.First();
		}
	}
}