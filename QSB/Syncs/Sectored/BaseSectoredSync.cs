using Mirror;
using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Syncs.Sectored
{
	public abstract class BaseSectoredSync : SyncBase
	{
		protected override bool AllowDisabledAttachedObject => false;
		protected override bool AllowNullReferenceTransform => true;

		public QSBSector ReferenceSector { get; private set; }
		public SectorSync.SectorSync SectorSync { get; private set; }

		private int _sectorId;

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

		protected override void GetFromAttached()
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

		protected override void ApplyToAttached()
		{
			if (_sectorId == -1)
			{
				return;
			}

			ReferenceSector = _sectorId.GetWorldObject<QSBSector>();
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
