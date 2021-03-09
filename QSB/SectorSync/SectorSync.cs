using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
	public class SectorSync : MonoBehaviour
	{
		public List<QSBSector> SectorList = new List<QSBSector>();

		private SectorDetector _sectorDetector;
		private readonly Sector.Name[] _sectorBlacklist = { Sector.Name.Ship };

		public void SetSectorDetector(SectorDetector detector)
		{
			if (_sectorDetector != null)
			{
				_sectorDetector.OnEnterSector -= AddSector;
				_sectorDetector.OnExitSector -= RemoveSector;
			}
			_sectorDetector = detector;
			_sectorDetector.OnEnterSector += AddSector;
			_sectorDetector.OnExitSector += RemoveSector;
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
			if (!QSBSectorManager.Instance.IsReady)
			{
				DebugLog.ToConsole($"Warning - Tried to get closest sector to {trans.name} before QSBSectorManager was ready.", MessageType.Warning);
				return null;
			}

			var listToCheck = SectorList.Count == 0
				? QSBWorldSync.GetWorldObjects<QSBSector>()
				: SectorList;
			// TODO : maybe improve this by more carefully picking sectors that are in the same place? maybe by last entered or closest to edge?
			return listToCheck
				.Where(sector => sector.AttachedObject != null
					&& !_sectorBlacklist.Contains(sector.Type)
					&& sector.Transform.gameObject.activeInHierarchy)
				.OrderBy(sector => Vector3.Distance(sector.Position, trans.position))
				.FirstOrDefault();
		}
	}
}