using QSB.Syncs.TransformSync;
using UnityEngine;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : SectoredTransformSync
	{
		public static ShipTransformSync LocalInstance { get; private set; }

		public override bool UseInterpolation => true;

		public override bool IsReady
			=> Locator.GetShipBody() != null;

		public override void OnStartLocalPlayer()
			=> LocalInstance = this;

		protected override GameObject InitLocalTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return Locator.GetShipBody().gameObject;
		}

		protected override GameObject InitRemoteTransform()
		{
			SectorSync.SetSectorDetector(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return Locator.GetShipBody().gameObject;
		}
	}
}