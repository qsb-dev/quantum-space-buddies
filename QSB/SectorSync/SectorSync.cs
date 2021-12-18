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
		public List<QSBSector> SectorList = new();

		private OWRigidbody _attachedOWRigidbody;
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

			_attachedOWRigidbody = _sectorDetector._attachedRigidbody;

			if (_attachedOWRigidbody == null)
			{
				DebugLog.ToConsole($"Warning - OWRigidbody for {_sectorDetector.name} is null!", MessageType.Warning);
			}

			PopulateSectorList();

			_targetType = type;
			IsReady = true;
		}

		private void PopulateSectorList()
		{
			var currentList = _sectorDetector._sectorList;

			SectorList.Clear();
			foreach (var sector in currentList)
			{
				if (sector == null)
				{
					continue;
				}

				AddSector(sector);
			}
		}

		private void AddSector(Sector sector)
		{
			var worldObject = QSBWorldSync.GetWorldFromUnity<QSBSector>(sector);
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

		public QSBSector GetClosestSector(Transform trans) // trans rights \o/
		{
			if (QSBSectorManager.Instance == null || !QSBSectorManager.Instance.IsReady)
			{
				return null;
			}

			if (!IsReady)
			{
				DebugLog.ToConsole($"Warning - Tried to use GetClosestSector() before this SectorSync is ready. Transform:{trans.name} Stacktrace:\r\n{Environment.StackTrace}", MessageType.Warning);
				return null;
			}

			if (_sectorDetector == null || _attachedOWRigidbody == null || _targetType == TargetType.None)
			{
				IsReady = false;
				return null;
			}

			bool ShouldSyncTo(QSBSector sector, TargetType type)
			{
				if (sector == null)
				{
					DebugLog.ToConsole($"Warning - Tried to check if we should sync to null sector!", MessageType.Warning);
					return false;
				}

				return sector.ShouldSyncTo(type);
			}

			var numSectorsCurrentlyIn = SectorList.Count(x => ShouldSyncTo(x, _targetType));

			var listToCheck = numSectorsCurrentlyIn == 0
				? QSBWorldSync.GetWorldObjects<QSBSector>().Where(x => !x.IsFakeSector && x.Type != Sector.Name.Unnamed)
				: SectorList;

			var activeNotNullNotBlacklisted = listToCheck.Where(sector => sector.AttachedObject != null
				&& sector.ShouldSyncTo(_targetType));

			if (activeNotNullNotBlacklisted.Count() == 0)
			{
				return default;
			}

			var ordered = activeNotNullNotBlacklisted
				.OrderBy(sector => CalculateSectorScore(sector, trans, _attachedOWRigidbody));

			var inASector = numSectorsCurrentlyIn != 0;

			bool IsApproxCloseToFirstSector(QSBSector sectorToCheck)
				=> OWMath.ApproxEquals(Vector3.Distance(sectorToCheck.Position, trans.position),
				Vector3.Distance(ordered.FirstOrDefault().Position, trans.position),
				0.01f);

			bool IsFakeSectorActive(QSBSector sectorToCheck)
				=> activeNotNullNotBlacklisted.Any(y => y.AttachedObject == (sectorToCheck.AttachedObject as FakeSector).AttachedSector);

			var fakeToSyncTo = QSBSectorManager.Instance.FakeSectors.FirstOrDefault(x => IsApproxCloseToFirstSector(x) && IsFakeSectorActive(x));

			if (!inASector)
			{
				fakeToSyncTo = default;
			}

			return fakeToSyncTo != default
				? fakeToSyncTo
				: ordered.FirstOrDefault();
		}

		public static float CalculateSectorScore(QSBSector sector, Transform trans, OWRigidbody rigidbody)
		{
			var distance = (sector.Position - trans.position).sqrMagnitude;
			var radius = GetRadius(sector);
			var velocity = GetRelativeVelocity(sector, rigidbody);

			return distance + Mathf.Pow(radius, 2) + velocity;
		}

		public static float GetRadius(QSBSector sector)
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

		public static float GetRelativeVelocity(QSBSector sector, OWRigidbody rigidbody)
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