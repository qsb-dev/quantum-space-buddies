using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync;

public class QSBSectorDetector : MonoBehaviour
{
	public readonly List<QSBSector> SectorList = new();

	private SectorDetector _sectorDetector;

	public void Init(SectorDetector detector)
	{
		if (_sectorDetector)
		{
			return;
		}

		if (!detector)
		{
			DebugLog.ToConsole($"Error - Trying to init QSBSectorDetector {name} with null SectorDetector!", MessageType.Error);
			return;
		}

		_sectorDetector = detector;
		_sectorDetector.OnEnterSector += AddSector;
		_sectorDetector.OnExitSector += RemoveSector;

		_sectorDetector._sectorList.ForEach(AddSector);
	}

	public void Uninit()
	{
		if (!_sectorDetector)
		{
			return;
		}

		_sectorDetector.OnEnterSector -= AddSector;
		_sectorDetector.OnExitSector -= RemoveSector;
		_sectorDetector = null;

		SectorList.Clear();
	}

	private void AddSector(Sector sector)
	{
		if (!sector)
		{
			// wtf
			DebugLog.ToConsole($"Warning - Trying to add null sector for QSBSectorDetector {name}", MessageType.Error);
			return;
		}

		var worldObject = sector.GetWorldObject<QSBSector>();
		if (worldObject == null)
		{
			DebugLog.ToConsole($"Error - Can't find QSBSector for sector {sector.name}!", MessageType.Error);
			return;
		}

		SectorList.SafeAdd(worldObject);
	}

	private void RemoveSector(Sector sector)
	{
		if (!sector)
		{
			// wtf
			DebugLog.ToConsole($"Warning - Trying to remove null sector for QSBSectorDetector {name}", MessageType.Error);
			return;
		}

		var worldObject = sector.GetWorldObject<QSBSector>();
		if (worldObject == null)
		{
			DebugLog.ToConsole($"Error - Can't find QSBSector for sector {sector.name}!", MessageType.Error);
			return;
		}

		SectorList.QuickRemove(worldObject);
	}

	/// <summary>
	/// called only by the sector manager
	/// </summary>
	public QSBSector GetClosestSector()
	{
		var type = _sectorDetector._occupantType;

		var validSectors = SectorList.Where(x => x.ShouldSyncTo(type)).ToList();
		var inASector = validSectors.Count > 0;

		if (!inASector)
		{
			validSectors = QSBWorldSync.GetWorldObjects<QSBSector>()
				.Where(x => !x.IsFakeSector && x.Type != Sector.Name.Unnamed && x.ShouldSyncTo(type))
				.ToList();
		}

		if (validSectors.Count == 0)
		{
			return null;
		}

		var closest = validSectors
			.MinBy(sector => CalculateSectorScore(sector, _sectorDetector._attachedRigidbody));

		if (inASector)
		{
			bool IsSamePositionAsClosest(QSBSector fakeSector)
				=> (fakeSector.Position - closest.Position).sqrMagnitude < 0.01f * 0.01f;

			bool IsAttachedValid(QSBSector fakeSector)
				=> validSectors.Any(x => x.AttachedObject == fakeSector.FakeSector.AttachedSector);

			var fakeToSyncTo = QSBSectorManager.Instance.FakeSectors
				.FirstOrDefault(x => IsSamePositionAsClosest(x) && IsAttachedValid(x));
			return fakeToSyncTo ?? closest;
		}

		return closest;
	}

	private static float CalculateSectorScore(QSBSector sector, OWRigidbody rigidbody)
	{
		var sqrDistance = (sector.Position - rigidbody.GetPosition()).sqrMagnitude;
		var radius = GetRadius(sector);
		var velocity = GetRelativeVelocity(sector, rigidbody);

		return sqrDistance + radius * radius + velocity;
	}

	private static float GetRadius(QSBSector sector)
	{
		// TODO : make this work for other stuff, not just shaped triggervolumes
		var trigger = sector.AttachedObject.GetTriggerVolume();
		if (trigger && trigger.GetShape())
		{
			return trigger.GetShape().CalcWorldBounds().radius;
		}

		return 0f;
	}

	private static float GetRelativeVelocity(QSBSector sector, OWRigidbody rigidbody)
	{
		var sectorRigidbody = sector.AttachedObject.GetOWRigidbody();
		if (sectorRigidbody && rigidbody)
		{
			var relativeVelocity = rigidbody.GetVelocity() - sectorRigidbody.GetPointVelocity(rigidbody.GetPosition());
			return relativeVelocity.sqrMagnitude;
		}

		return 0;
	}
}
