using QSB.ShipSync.WorldObjects;
using QSB.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : WorldObjectTransformSync
	{
		public override SyncType SyncType => SyncType.Ship;

		protected override IWorldObject GetWorldObject()
		{
			SectorSync.SetSectorDetector(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return QSBWorldSync.GetWorldFromId<QSBShip>(0);
		}

		public override bool IsReady 
			=> QSBWorldSync.GetWorldObjects<QSBShip>().Count() != 0;
	}
}