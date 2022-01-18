using Mirror;
using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Syncs.Sectored
{
	public abstract class BaseSectoredSync : SyncBase
	{
		protected override bool AllowNullReferenceTransform => true;

		public QSBSector ReferenceSector { get; private set; }
		public SectorSync.SectorSync SectorSync { get; private set; }

		private int _sectorId = -1;

		public override void OnStartClient()
		{
			SectorSync = gameObject.AddComponent<SectorSync.SectorSync>();
			QSBSectorManager.Instance.TransformSyncs.Add(this);
			base.OnStartClient();
		}

		public override void OnStopClient()
		{
			base.OnStopClient();
			QSBSectorManager.Instance.TransformSyncs.Remove(this);
			Destroy(SectorSync);
		}

		protected override void OnSceneLoaded(OWScene oldScene, OWScene newScene, bool isInUniverse)
		{
			base.OnSceneLoaded(oldScene, newScene, isInUniverse);
			SetReferenceSector(null);
		}

		protected override void Serialize(NetworkWriter writer, bool initialState)
		{
			base.Serialize(writer, initialState);
			writer.Write(_sectorId);
		}

		protected override void Deserialize(NetworkReader reader, bool initialState)
		{
			base.Deserialize(reader, initialState);
			_sectorId = reader.ReadInt();
		}

		protected void GetFromSector()
		{
			if (ReferenceSector != null)
			{
				_sectorId = ReferenceSector.ObjectId;
			}
			else
			{
				_sectorId = -1;
			}
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
