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

		private OWRigidbody _body;
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

		public void Init(SectorDetector detector, OWRigidbody body, TargetType type)
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

			_body = body;
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
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBSector>(sector);
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
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBSector>(sector);
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

			var useSectorList = SectorList.Any(x => x.ShouldSyncTo(_targetType));
			var listToCheck = useSectorList
				? SectorList
				: QSBWorldSync.GetWorldObjects<QSBSector>().Where(x => !x.IsFakeSector && x.Type != Sector.Name.Unnamed);

			var goodSectors = listToCheck
				.Where(x => x.ShouldSyncTo(_targetType))
				.ToList();
			if (goodSectors.Count == 0)
			{
				return default;
			}

			var closest = goodSectors
				.OrderBy(CalculateSectorScore).First();

			if (useSectorList)
			{
				var pos = _body.GetPosition();
				// if any fake sectors are *roughly* in the same place as other sectors - we want fake sectors to override other sectors
				var fakeSector = QSBSectorManager.Instance.FakeSectors.FirstOrDefault(x =>
					OWMath.ApproxEquals(Vector3.Distance(x.Position, pos), Vector3.Distance(closest.Position, pos), 0.01f) &&
					goodSectors.Any(y => x.FakeSector.AttachedSector == y.AttachedObject)
				);
				return fakeSector ?? closest;
			}

			return closest;
		}

		private float CalculateSectorScore(QSBSector sector)
		{
			var distance = Vector3.Distance(sector.Position, _body.GetPosition()); // want to be small
			var radius = GetRadius(sector); // want to be small
			var velocity = GetRelativeVelocity(sector); // want to be small

			return Mathf.Pow(distance, 2) + Mathf.Pow(radius, 2) + Mathf.Pow(velocity, 2);
		}

		private static float GetRadius(QSBSector sector)
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

		private float GetRelativeVelocity(QSBSector sector)
		{
			var sectorRigidBody = sector.AttachedObject.GetOWRigidbody();
			if (sectorRigidBody != null && _body != null)
			{
				var relativeVelocity = sectorRigidBody.GetRelativeVelocity(_body);
				var relativeVelocityMagnitude = relativeVelocity.sqrMagnitude; // Remember this is squared for efficiency!
				return relativeVelocityMagnitude;
			}

			return 0;
		}
	}
}
