using QSB.Syncs.TransformSync;
using QSB.Utility;
using UnityEngine;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : SectoredTransformSync
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

		protected override void UpdateTransform()
		{
			base.UpdateTransform();

			if (!HasAuthority && ReferenceSector != null)
			{
				Locator.GetShipBody().SetVelocity(ReferenceSector.AttachedObject.GetOWRigidbody().GetPointVelocity(Locator.GetShipTransform().position));
			}
		}
	}
}