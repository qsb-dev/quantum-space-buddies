using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
	public class QSBSectorManager : MonoBehaviour
	{
		public static QSBSectorManager Instance { get; private set; }

		public List<QSBSector> SectorList = new List<QSBSector>();
		public bool IsReady { get; private set; }

		private readonly Sector.Name[] _sectorBlacklist =
		{
			Sector.Name.Ship
		};

		public void Awake()
		{
			Instance = this;
			QSBSceneManager.OnUniverseSceneLoaded += (OWScene scene) => RebuildSectors();
			DebugLog.DebugWrite("Sector Manager ready.", MessageType.Success);
		}

		public void OnDestroy()
		{
			QSBSceneManager.OnUniverseSceneLoaded -= (OWScene scene) => RebuildSectors();
			FindObjectOfType<PlayerSectorDetector>().OnEnterSector -= AddSector;
			FindObjectOfType<PlayerSectorDetector>().OnExitSector -= RemoveSector;
		}

		public void RebuildSectors()
		{
			DebugLog.DebugWrite("Rebuilding sectors...", MessageType.Warning);
			QSBWorldSync.Init<QSBSector, Sector>();
			SectorList.Clear();
			IsReady = QSBWorldSync.GetWorldObjects<QSBSector>().Any();

			FindObjectOfType<PlayerSectorDetector>().OnEnterSector += AddSector;
			FindObjectOfType<PlayerSectorDetector>().OnExitSector += RemoveSector;
		}

		private void AddSector(Sector sector)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBSector, Sector>(sector);
			if (worldObject == null)
			{
				DebugLog.ToConsole($"Error - Can't find QSBSector for sector {sector.name}!", MessageType.Error);
			}
			if (SectorList.Contains(worldObject))
			{
				DebugLog.ToConsole($"Warning - Trying to add {sector.name}, but is already in list", MessageType.Warning);
				return;
			}
			SectorList.Add(worldObject);
		}

		private void RemoveSector(Sector sector)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBSector, Sector>(sector);
			if (worldObject == null)
			{
				DebugLog.ToConsole($"Error - Can't find QSBSector for sector {sector.name}!", MessageType.Error);
				return;
			}
			if (!SectorList.Contains(worldObject))
			{
				DebugLog.ToConsole($"Warning - Trying to remove {sector.name}, but is not in list!", MessageType.Warning);
				return;
			}
			SectorList.Remove(worldObject);
		}

		public QSBSector GetClosestSector(Transform trans) // trans rights \o/
		{
			if (!IsReady)
			{
				DebugLog.ToConsole($"Warning - Tried to get closest sector to {trans.name} before SectorManager was ready.", MessageType.Warning);
				return null;
			}

			var listToCheck = SectorList.Count == 0
				? QSBWorldSync.GetWorldObjects<QSBSector>()
				: SectorList;
			return listToCheck
				.Where(sector => sector.AttachedObject != null
					&& !_sectorBlacklist.Contains(sector.Type)
					&& sector.Transform.gameObject.activeInHierarchy)
				.OrderBy(sector => Vector3.Distance(sector.Position, trans.position))
				.FirstOrDefault();
		}
	}
}