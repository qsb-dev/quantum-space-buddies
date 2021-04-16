using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.TransformSync;
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
		private ITransformSync _owner;

		private void OnDestroy()
		{
			if (_sectorDetector != null)
			{
				_sectorDetector.OnEnterSector -= AddSector;
				_sectorDetector.OnExitSector -= RemoveSector;
			}
		}

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

		public void SetOwner(ITransformSync owner)
		{
			if (owner == null)
			{
				DebugLog.ToConsole($"Warning - Trying to set owner of a SectorSync to a null value.", MessageType.Warning);
			}
			if (_owner != null)
			{
				DebugLog.ToConsole($"Warning - Trying to set owner of a SectorSync that already has an owner.", MessageType.Warning);
			}
			_owner = owner;
		}

		public ITransformSync GetOwner() => _owner;

		private void AddSector(Sector sector)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBSector, Sector>(sector);
			if (worldObject == null)
			{
				DebugLog.ToConsole($"Error - Can't find QSBSector for sector {sector.name}!", MessageType.Error);
			}
			if (SectorList.Contains(worldObject))
			{
				DebugLog.ToConsole($"Warning - Trying to add {sector.name} for {gameObject.name}, but is already in list", MessageType.Warning);
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
				DebugLog.ToConsole($"Warning - Trying to remove {sector.name} for {gameObject.name}, but is not in list!", MessageType.Warning);
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

			var listToCheck = SectorList.Count(x => x.ShouldSyncTo(_owner)) == 0
				? QSBWorldSync.GetWorldObjects<QSBSector>()
				: SectorList;

			/* Explanation of working out which sector to sync to :
			 * A) Closer sectors are best
			 * B) Smaller sub-sectors are preferred
			 * So, get all non-null sectors that aren't blacklisted and are active
			 * (They need to be active otherwise it'll sync to disabled sectors, like the eye shuttle - which makes the player invisible)
			 * Then, sort that list also by the radius of the sector.
			 * We want smaller subsectors (e.g. Starting_Camp) to be preferred over general sectors (e.g. Village)
			 * TL;DR : Sync to the smallest, closest sector
			 */

			var activeNotNullNotBlacklisted = listToCheck.Where(sector => sector.AttachedObject != null
				&& sector.ShouldSyncTo(_owner));
			if (activeNotNullNotBlacklisted.Count() == 0)
			{
				return default;
			}
			var ordered = activeNotNullNotBlacklisted
				.OrderBy(sector => Vector3.Distance(sector.Position, trans.position))
				.ThenBy(sector => GetRadius(sector));

			if (
				// if any fake sectors are *roughly* in the same place as other sectors - we want fake sectors to override other sectors
				QSBSectorManager.Instance.FakeSectors.Any(
					x => OWMath.ApproxEquals(Vector3.Distance(x.Position, trans.position), Vector3.Distance(ordered.FirstOrDefault().Position, trans.position), 0.01f)
				&& activeNotNullNotBlacklisted.Any(
					y => y.AttachedObject == (x.AttachedObject as FakeSector).AttachedSector)))
			{
				return QSBSectorManager.Instance.FakeSectors.First(x => OWMath.ApproxEquals(Vector3.Distance(x.Position, trans.position), Vector3.Distance(ordered.FirstOrDefault().Position, trans.position), 0.01f));
			}

			return ordered.FirstOrDefault();
		}

		private float GetRadius(QSBSector sector)
		{
			if (sector == null)
			{
				return 0f;
			}
			// TODO : make this work for other stuff, not just shaped triggervolumes
			var trigger = sector.AttachedObject.GetTriggerVolume();
			if (trigger != null)
			{
				if (trigger.GetShape() != null)
				{
					return trigger.GetShape().CalcWorldBounds().radius;
				}
			}
			return 0f;
		}
	}
}