using Mirror;
using OWML.Common;
using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.Syncs.Sectored
{
	public abstract class BaseSectoredSync : SyncBase
	{
		public override bool AllowDisabledAttachedObject => false;
		public override bool AllowNullReferenceTransform => true;

		public QSBSector ReferenceSector { get; private set; }
		public SectorSync.SectorSync SectorSync { get; private set; }

		public override void Start()
		{
			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();
			QSBSectorManager.Instance.TransformSyncs.Add(this);
			base.Start();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			QSBSectorManager.Instance.TransformSyncs.Remove(this);
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

		protected override void Update()
		{
			if (ReferenceSector != null && ReferenceSector.Transform != ReferenceTransform)
			{
				DebugLog.ToConsole($"Warning - {LogName} : ReferenceSector.Transform was different to ReferenceTransform. Correcting...", MessageType.Warning);
				SetReferenceTransform(ReferenceSector.Transform);
			}

			// todo all the check in base
			base.Update();
		}

		protected override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);

			if (ReferenceSector == null)
			{
				DebugLog.ToConsole($"Warning - ReferenceSector of {LogName} is null.", MessageType.Warning);
				writer.Write(-1);
				return;
			}

			writer.Write(ReferenceSector.ObjectId);
		}

		protected override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);

			var sectorId = reader.ReadInt();
			if (sectorId == -1)
			{
				DebugLog.ToConsole($"Error - {LogName} got sector of ID -1. (From deserializing transform.)", MessageType.Error);
				return;
			}

			var sector = sectorId.GetWorldObject<QSBSector>();
			SetReferenceSector(sector);
		}

		public void SetReferenceSector(QSBSector sector)
		{
			if (ReferenceSector == sector)
			{
				return;
			}

			ReferenceSector = sector;
			SetReferenceTransform(sector?.Transform);
		}
	}
}
