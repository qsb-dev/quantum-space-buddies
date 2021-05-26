using QSB.Player;
using QSB.SectorSync;
using QSB.Syncs.RigidbodySync;
using QSB.Utility;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : SectoredRigidbodySync
	{
		public static ShipTransformSync LocalInstance { get; private set; }

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
			SectorSync.Init(Locator.GetShipDetector().GetComponent<SectorDetector>(), this);
			return Locator.GetShipBody();
		}

		protected override void UpdateTransform()
		{
			if (HasAuthority && ShipManager.Instance.CurrentFlyer != QSBPlayerManager.LocalPlayerId)
			{
				return;
			}

			if (!HasAuthority && ShipManager.Instance.CurrentFlyer == QSBPlayerManager.LocalPlayerId)
			{
				return;
			}

			base.UpdateTransform();
		}

		public override TargetType Type => TargetType.Ship;

		public override bool UseInterpolation => true;
		protected override float DistanceLeeway => 20f;
	}
}