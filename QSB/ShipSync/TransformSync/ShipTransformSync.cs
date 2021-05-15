using QSB.Player;
using QSB.SectorSync;
using QSB.Syncs.RigidbodySync;
using QSB.Syncs.TransformSync;
using QSB.Utility;
using UnityEngine;

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
				DebugLog.DebugWrite($"Warning - Local player has ship authority, but is not the current flyer!", OWML.Common.MessageType.Warning);
				return;
			}

			if (!HasAuthority && ShipManager.Instance.CurrentFlyer == QSBPlayerManager.LocalPlayerId)
			{
				DebugLog.DebugWrite($"Warning - Local player does not have ship authority, but is the current flyer!", OWML.Common.MessageType.Warning);
				return;
			}

			base.UpdateTransform();
		}

		public override TargetType Type => TargetType.Ship;

		public override bool UseInterpolation => true;
	}
}