using QSB.SectorSync;
using QSB.Syncs.Sectored.Rigidbodies;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : SectoredRigidbodySync
	{
		public static ShipTransformSync LocalInstance { get; private set; }

		private const int ForcePositionAfterUpdates = 50;
		private int _updateCount;

		public override bool IsReady
			=> Locator.GetShipBody() != null;

		public override void Start()
		{
			base.Start();
			LocalInstance = this;
		}

		protected override OWRigidbody GetRigidbody()
		{
			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllReady, () => SectorSync.Init(Locator.GetShipDetector().GetComponent<SectorDetector>(), TargetType.Ship));
			return Locator.GetShipBody();
		}

		private void ForcePosition()
		{
			var targetPos = _intermediaryTransform.GetTargetPosition_Unparented();
			var targetRot = _intermediaryTransform.GetTargetRotation_Unparented();

			(AttachedObject as OWRigidbody).SetPosition(targetPos);
			(AttachedObject as OWRigidbody).SetRotation(targetRot);
		}

		protected override bool UpdateTransform()
		{
			if (HasAuthority)
			{
				_intermediaryTransform.EncodePosition(AttachedObject.transform.position);
				_intermediaryTransform.EncodeRotation(AttachedObject.transform.rotation);
				_relativeVelocity = GetRelativeVelocity();
				_relativeAngularVelocity = (AttachedObject as OWRigidbody).GetRelativeAngularVelocity(ReferenceTransform.GetAttachedOWRigidbody());
				return true;
			}

			_updateCount++;

			if (_updateCount >= ForcePositionAfterUpdates)
			{
				_updateCount = 0;
				ForcePosition();
			}

			var targetPos = _intermediaryTransform.GetTargetPosition_Unparented();

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().GetPointVelocity(targetPos) + _relativeVelocity;
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().GetAngularVelocity() + _relativeAngularVelocity;

			SetVelocity(AttachedObject as OWRigidbody, targetVelocity);
			(AttachedObject as OWRigidbody).SetAngularVelocity(targetAngularVelocity);

			return true;
		}

		public override bool UseInterpolation => true;
		protected override float DistanceLeeway => 20f;
	}
}