using QSB.Player;
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
			=> Locator.GetShipBody() != null;

		public override void OnStartLocalPlayer()
			=> LocalInstance = this;

		protected override OWRigidbody InitLocalTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return Locator.GetShipBody();
		}

		protected override OWRigidbody InitRemoteTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return Locator.GetShipBody();
		}
	}
}