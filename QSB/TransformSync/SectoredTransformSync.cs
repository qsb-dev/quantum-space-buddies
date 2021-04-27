using QSB.SectorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET.Transport;
using System.Collections.Generic;

namespace QSB.TransformSync
{
	public abstract class SectoredTransformSync : BaseTransformSync
	{
		public QSBSector ReferenceSector { get; set; }
		public SectorSync.SectorSync SectorSync { get; private set; }
		public static List<SectoredTransformSync> SectoredNetworkTransformList = new List<SectoredTransformSync>();

		public override void Start()
		{
			DebugLog.DebugWrite($"Start of {GetType().Name}");
			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();
			SectoredNetworkTransformList.Add(this);
			base.Start();
		}

		protected override void OnDestroy()
		{
			DebugLog.DebugWrite($"OnDestroy of {GetType().Name}");
			base.OnDestroy();
			SectoredNetworkTransformList.Remove(this);
			if (SectorSync != null)
			{
				Destroy(SectorSync);
			}
		}

		protected override void Init()
		{
			DebugLog.DebugWrite($"Init of {GetType().Name}");
			base.Init();
			SetReferenceTransform(SectorSync.GetClosestSector(AttachedObject.transform).Transform);
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
			if (!QSBCore.HasWokenUp)
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

		public void SetReferenceSector(QSBSector sector)
		{
			ReferenceSector = sector;
			SetReferenceTransform(sector.Transform);
		}
	}
}
