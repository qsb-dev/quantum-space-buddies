using Mirror;
using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Syncs.Sectored
{
	public abstract class BaseSectoredSync : SyncBase
	{
		protected sealed override bool AllowNullReferenceTransform => true;

		public QSBSector ReferenceSector { get; private set; }
		public QSBSectorDetector SectorDetector { get; private set; }

		private int _sectorId = -1;

		public override void OnStartClient()
		{
			SectorDetector = gameObject.AddComponent<QSBSectorDetector>();
			QSBSectorManager.Instance.SectoredSyncs.Add(this);
			base.OnStartClient();
		}

		public override void OnStopClient()
		{
			base.OnStopClient();
			QSBSectorManager.Instance.SectoredSyncs.Remove(this);
			Destroy(SectorDetector);
		}

		protected override void Uninit()
		{
			base.Uninit();

			SectorDetector.Uninit();
			SetReferenceSector(null);
		}

		protected override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(_sectorId);
		}

		protected override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			_sectorId = reader.ReadInt();
		}

		protected void GetFromSector()
		{
			_sectorId = ReferenceSector?.ObjectId ?? -1;
		}

		protected void ApplyToSector()
		{
			if (_sectorId == -1)
			{
				return;
			}

			SetReferenceSector(_sectorId.GetWorldObject<QSBSector>());
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
