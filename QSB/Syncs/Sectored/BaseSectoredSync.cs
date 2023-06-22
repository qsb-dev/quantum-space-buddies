using Mirror;
using QSB.SectorSync;
using QSB.SectorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.Syncs.Sectored;

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

	protected void GetFromSector() => _sectorId = ReferenceSector?.ObjectId ?? -1;

	protected override void Serialize(NetworkWriter writer)
	{
		writer.Write(_sectorId);
		base.Serialize(writer);
	}

	protected override void Deserialize(NetworkReader reader)
	{
		_sectorId = reader.ReadInt();
		base.Deserialize(reader);
	}

	protected void ApplyToSector()
	{
		if (_sectorId == -1)
		{
			return;
		}

		SetReferenceSector(_sectorId.GetWorldObject<QSBSector>());
	}

	/// <summary>
	/// use this instead of SetReferenceTransform
	/// <para/>
	/// called by QSBSectorManager (owner) and ApplyToSector (non owner)
	/// </summary>
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