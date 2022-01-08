using OWML.Common;
using QSB.Player;
using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Syncs.Sectored
{
	public interface IBaseSectoredSync
	{
		Component ReturnObject();
		bool HasAuthority { get; }
		bool IsReady { get; }
		SectorSync.SectorSync SectorSync { get; }
		QSBSector ReferenceSector { get; }
		void SetReferenceSector(QSBSector closestSector);
	}

	public abstract class BaseSectoredSync<T> : SyncBase<T>, IBaseSectoredSync where T : Component
	{
		public override bool IgnoreDisabledAttachedObject => false;
		public override bool IgnoreNullReferenceTransform => true;

		public Component ReturnObject() => AttachedObject;
		public QSBSector ReferenceSector { get; set; }
		public SectorSync.SectorSync SectorSync { get; private set; }

		private int _sectorIdWaitingSlot = int.MinValue;

		public override void Start()
		{
			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();
			QSBSectorManager.Instance.SectoredSyncs.Add(this);
			base.Start();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			QSBSectorManager.Instance.SectoredSyncs.Remove(this);
			if (SectorSync != null)
			{
				Destroy(SectorSync);
			}
		}

		protected override void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isInUniverse)
		{
			base.OnSceneLoaded(oldScene, newScene, isInUniverse);
			SetReferenceSector(null);
		}

		protected override void Init()
		{
			base.Init();
			if (!QSBSectorManager.Instance.IsReady)
			{
				return;
			}

			if (!HasAuthority)
			{
				return;
			}

			QSBCore.UnityEvents.RunWhen(() => SectorSync.IsReady, InitSector);
		}

		private void InitSector()
		{
			var closestSector = SectorSync.GetClosestSector();
			if (closestSector != null)
			{
				SetReferenceSector(closestSector);
			}
			else
			{
				DebugLog.ToConsole($"Warning - {LogName}'s initial sector was null.", MessageType.Warning);
			}
		}

		public override void Update()
		{
			if (_sectorIdWaitingSlot == int.MinValue)
			{
				if (ReferenceSector != null && ReferenceSector.Transform != ReferenceTransform)
				{
					DebugLog.ToConsole($"Warning - {LogName} : ReferenceSector.Transform was different to ReferenceTransform. Correcting...", MessageType.Warning);
					SetReferenceTransform(ReferenceSector.Transform);
				}

				base.Update();
				return;
			}

			if (!WorldObjectManager.AllObjectsReady)
			{
				base.Update();
				return;
			}

			var sector = _sectorIdWaitingSlot == -1
				? null
				: _sectorIdWaitingSlot.GetWorldObject<QSBSector>();

			if (sector != ReferenceSector)
			{
				if (sector == null)
				{
					DebugLog.ToConsole($"Error - {LogName} got sector of ID -1. (From waiting slot.)", MessageType.Error);
					base.Update();
					return;
				}

				SetReferenceSector(sector);
			}

			_sectorIdWaitingSlot = int.MinValue;

			base.Update();
		}

		public override void SerializeTransform(QNetworkWriter writer, bool initialState)
		{
			if (IsPlayerObject)
			{
				if (!Player.IsReady)
				{
					writer.Write(-1);
					return;
				}
			}

			if (ReferenceSector != null)
			{
				writer.Write(ReferenceSector.ObjectId);
			}
			else
			{
				if (_isInitialized)
				{
					DebugLog.ToConsole($"Warning - ReferenceSector of {LogName} is null.", MessageType.Warning);
				}

				writer.Write(-1);
			}
		}

		public override void DeserializeTransform(QNetworkReader reader, bool initialState)
		{
			if (HasAuthority)
			{
				reader.ReadInt32();
				return;
			}

			int sectorId;
			if (!WorldObjectManager.AllObjectsReady)
			{
				sectorId = reader.ReadInt32();
				if (initialState && sectorId != -1)
				{
					_sectorIdWaitingSlot = sectorId;
				}

				return;
			}

			sectorId = reader.ReadInt32();
			var sector = sectorId == -1
				? null
				: sectorId.GetWorldObject<QSBSector>();

			if (sector != ReferenceSector)
			{
				if (sector == null)
				{
					DebugLog.ToConsole($"Error - {LogName} got sector of ID -1. (From deserializing transform.)", MessageType.Error);
					return;
				}

				SetReferenceSector(sector);
			}
		}

		protected bool UpdateSectors()
		{
			var referenceNull = ReferenceTransform == null || ReferenceSector == null;
			var sectorManagerReady = QSBSectorManager.Instance.IsReady;

			if (!sectorManagerReady)
			{
				if (referenceNull && HasAuthority)
				{
					DebugLog.ToConsole($"Warning - Reference was null, but sector manager wasn't ready. " +
						$"Transform:{ReferenceTransform == null}, Sector:{ReferenceSector == null}",
						MessageType.Warning);
				}

				DebugLog.DebugWrite($"{LogName} : Sector Manager not ready.");
				return false;
			}

			if (!HasAuthority)
			{
				return true;
			}

			if (referenceNull)
			{
				if (SectorSync.IsReady)
				{
					var closestSector = SectorSync.GetClosestSector();
					if (closestSector != null)
					{
						SetReferenceSector(closestSector);
						return true;
					}
					else
					{
						DebugLog.ToConsole($"Error - No closest sector found to {LogName}!", MessageType.Error);
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		protected override bool UpdateTransform()
			=> UpdateSectors();

		public void SetReferenceSector(QSBSector sector)
		{
			ReferenceSector = sector;
			SetReferenceTransform(sector?.Transform);
		}
	}
}
