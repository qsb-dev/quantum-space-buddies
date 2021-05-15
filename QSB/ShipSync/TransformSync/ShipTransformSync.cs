using QSB.Syncs.RigidbodySync;
using QSB.Syncs.TransformSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : SectoredRigidbodySync
	{
		public static ShipTransformSync LocalInstance { get; private set; }

		public override bool UseInterpolation => true;

		public override bool IsReady
			=> Locator.GetShipBody() != null;

		public override void Start()
		{
			DebugLog.DebugWrite($"START!");
			base.Start();
			LocalInstance = this;
		}

		protected override OWRigidbody GetRigidbody()
		{
			SectorSync.SetSectorDetector(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return Locator.GetShipBody();
		}
	}
}