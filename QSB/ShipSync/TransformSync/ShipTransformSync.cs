using QSB.SectorSync;
using QSB.Syncs;
using QSB.Syncs.Sectored.Rigidbodies;
using QSB.WorldSync;
using UnityEngine;

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
			if (ReferenceTransform == null)
			{
				return;
			}

			var targetPos = ReferenceTransform.DecodePos(transform.position);
			var targetRot = ReferenceTransform.DecodeRot(transform.rotation);

			((ShipBody)AttachedObject).SetPosition(targetPos);
			((ShipBody)AttachedObject).SetRotation(targetRot);
		}

		protected override bool UpdateTransform()
		{
			if (!UpdateSectors())
			{
				return false;
			}

			// Dont do base... this is a replacement!

			if (HasAuthority)
			{
				SetValuesToSync();
				return true;
			}

			_updateCount++;

			if (_updateCount >= ForcePositionAfterUpdates)
			{
				_updateCount = 0;
				ForcePosition();
			}

			if (ReferenceTransform == null)
			{
				return true;
			}

			var targetPos = ReferenceTransform.DecodePos(transform.position);

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().DecodeVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().DecodeAngVel(_relativeAngularVelocity);

			SetVelocity((ShipBody)AttachedObject, targetVelocity);
			((ShipBody)AttachedObject).SetAngularVelocity(targetAngularVelocity);

			return true;
		}

		/// use OWRigidbody version instead of ShipBody override
		private void SetVelocity(OWRigidbody rigidbody, Vector3 newVelocity)
		{
			if (rigidbody.RunningKinematicSimulation())
			{
				rigidbody._kinematicRigidbody.velocity = newVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
			}
			else
			{
				rigidbody._rigidbody.velocity = newVelocity + Locator.GetCenterOfTheUniverse().GetStaticFrameVelocity_Internal();
			}
			rigidbody._lastVelocity = rigidbody._currentVelocity;
			rigidbody._currentVelocity = newVelocity;
		}

		public override bool UseInterpolation => true;
		protected override float DistanceLeeway => 20f;
	}
}
