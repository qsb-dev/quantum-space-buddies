using QSB.Player;
using QSB.ShipSync.WorldObjects;
using QSB.Syncs.TransformSync;
using QSB.WorldSync;
using System.Linq;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : UnparentedSectoredRigidbodySync
	{
		public static ShipTransformSync LocalInstance { get; private set; }

		public override bool UseInterpolation => true;

		public override bool IsReady 
			=> QSBWorldSync.GetWorldObjects<QSBShip>().Count() != 0;

		public override void OnStartLocalPlayer()
			=> LocalInstance = this;

		protected override OWRigidbody InitLocalTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return QSBWorldSync.GetWorldFromId<QSBShip>(0).AttachedObject;
		}

		protected override OWRigidbody InitRemoteTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return QSBWorldSync.GetWorldFromId<QSBShip>(0).AttachedObject;
		}
	}
}