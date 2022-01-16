using OWML.Common;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.SectorSync
{
	public class SectorSync : MonoBehaviour
	{
		public bool IsReady { get; private set; }
		public readonly List<QSBSector> SectorList = new();

		private SectorDetector _sectorDetector;
		private TargetType _targetType;

		private void OnDestroy()
		{
			if (_sectorDetector != null)
			{
				_sectorDetector.OnEnterSector -= AddSector;
				_sectorDetector.OnExitSector -= RemoveSector;
			}

			IsReady = false;
		}

		public void Init(SectorDetector detector, TargetType type)
		{
			if (_sectorDetector != null)
			{
				_sectorDetector.OnEnterSector -= AddSector;
				_sectorDetector.OnExitSector -= RemoveSector;
			}

			if (detector == null)
			{
				DebugLog.ToConsole($"Error - Trying to init SectorSync with null SectorDetector.", MessageType.Error);
				return;
			}

			_sectorDetector = detector;
			_sectorDetector.OnEnterSector += AddSector;
			_sectorDetector.OnExitSector += RemoveSector;

			SectorList.Clear();
			_sectorDetector._sectorList.ForEach(AddSector);

			_targetType = type;
			IsReady = true;
		}

		private void AddSector(Sector sector)
		{
			var worldObject = sector.GetWorldObject<QSBSector>();
			if (worldObject == null)
			{
				DebugLog.ToConsole($"Error - Can't find QSBSector for sector {sector.name}!", MessageType.Error);
				return;
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
			var worldObject = sector.GetWorldObject<QSBSector>();
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

		public QSBSector GetClosestSector()
		{
			if (QSBSectorManager.Instance == null || !QSBSectorManager.Instance.IsReady)
			{
				return null;
			}

			if (!IsReady)
			{
				DebugLog.ToConsole($"Warning - Tried to use GetClosestSector() before this SectorSync is ready. Stacktrace:\r\n{Environment.StackTrace}", MessageType.Warning);
				return null;
			}

			if (_sectorDetector == null)
			{
				IsReady = false;
				return null;
			}

			var inASector = SectorList.Any(x => x.ShouldSyncTo(_targetType));

			var listToCheck = inASector
				? SectorList
				: QSBWorldSync.GetWorldObjects<QSBSector>().Where(x => !x.IsFakeSector && x.Type != Sector.Name.Unnamed);

			var goodSectors = listToCheck.Where(sector => sector.ShouldSyncTo(_targetType)).ToList();

			if (goodSectors.Count == 0)
			{
				return default;
			}

			var closest = goodSectors
				.OrderBy(sector => CalculateSectorScore(sector, _sectorDetector._attachedRigidbody)).First();

			if (inASector)
			{
				var pos = _sectorDetector._attachedRigidbody.GetPosition();

				bool IsApproxCloseToClosestSector(QSBSector sectorToCheck)
					=> OWMath.ApproxEquals(Vector3.Distance(sectorToCheck.Position, pos),
					Vector3.Distance(closest.Position, pos),
					0.01f);

				bool IsFakeSectorActive(QSBSector fakeSectorToCheck)
					=> goodSectors.Any(x => fakeSectorToCheck.FakeSector.AttachedSector == x.AttachedObject);

				var fakeToSyncTo = QSBSectorManager.Instance.FakeSectors.FirstOrDefault(x => IsApproxCloseToClosestSector(x) && IsFakeSectorActive(x));
				return fakeToSyncTo ?? closest;
			}

			return closest;
		}

		private static float CalculateSectorScore(QSBSector sector, OWRigidbody rigidbody)
		{
			var distance = (sector.Position - rigidbody.GetPosition()).sqrMagnitude;
			var radius = GetRadius(sector);
			var velocity = GetRelativeVelocity(sector, rigidbody);

			return distance + Mathf.Pow(radius, 2) + velocity;
		}

		private static float GetRadius(QSBSector sector)
		{
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

		private static float GetRelativeVelocity(QSBSector sector, OWRigidbody rigidbody)
		{
			var sectorRigidBody = sector.AttachedObject.GetOWRigidbody();
			if (sectorRigidBody != null && rigidbody != null)
			{
				var relativeVelocity = sectorRigidBody.GetRelativeVelocity(rigidbody);
				return relativeVelocity.sqrMagnitude;
			}

			return 0;
		}
	}
}