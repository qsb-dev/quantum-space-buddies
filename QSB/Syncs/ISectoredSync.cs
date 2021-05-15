using QSB.SectorSync.WorldObjects;

namespace QSB.Syncs
{
	public interface ISectoredSync<T> : ISync<T>
	{
		SectorSync.SectorSync SectorSync { get; }
		QSBSector ReferenceSector { get; }

		void SetReferenceSector(QSBSector sector);
	}
}
