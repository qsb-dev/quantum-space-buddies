using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Syncs.TransformSync
{
	public abstract class UnparentedSectoredRigidbodySync : UnparentedBaseRigidBodySync
	{
		public QSBSector ReferenceSector { get; set; }
		public SectorSync.SectorSync SectorSync { get; private set; }
		public static List<UnparentedSectoredRigidbodySync> UnparentedSectoredNetworkRigidbodyList = new List<UnparentedSectoredRigidbodySync>();

		public override void Start()
		{
			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();
			UnparentedSectoredNetworkRigidbodyList.Add(this);
			base.Start();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			UnparentedSectoredNetworkRigidbodyList.Remove(this);
			if (SectorSync != null)
			{
				Destroy(SectorSync);
			}
		}

		protected override void Init()
		{
			base.Init();
			if (!QSBSectorManager.Instance.IsReady)
			{
				return;
			}
			var closestSector = SectorSync.GetClosestSector(AttachedObject.transform);
			if (closestSector != null)
			{
				SetReferenceTransform(closestSector.Transform);
			}
		}

		public override void SerializeTransform(QNetworkWriter writer)
		{
			if (_intermediaryTransform == null)
			{
				_intermediaryTransform = new IntermediaryTransform(transform);
			}
			if (ReferenceSector != null)
			{
				writer.Write(ReferenceSector.ObjectId);
			}
			else
			{
				writer.Write(-1);
			}
			base.SerializeTransform(writer);
		}

		public override void DeserializeTransform(QNetworkReader reader)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				reader.ReadInt32();
				reader.ReadVector3();
				DeserializeRotation(reader);
				return;
			}

			var sectorId = reader.ReadInt32();
			var sector = sectorId == -1
				? null
				: QSBWorldSync.GetWorldFromId<QSBSector>(sectorId);

			if (sector != ReferenceSector)
			{
				SetReferenceSector(sector);
			}

			base.DeserializeTransform(reader);
		}

		protected override void UpdateTransform()
		{
			if ((ReferenceTransform == null || ReferenceSector == null) && QSBSectorManager.Instance.IsReady)
			{
				var closestSector = SectorSync.GetClosestSector(AttachedObject.transform);
				if (closestSector != null)
				{
					SetReferenceTransform(closestSector.Transform);
				}
				else
				{
					return;
				}
			}

			base.UpdateTransform();
		}

		public void SetReferenceSector(QSBSector sector)
		{
			ReferenceSector = sector;
			SetReferenceTransform(sector.Transform);
		}
	}
}