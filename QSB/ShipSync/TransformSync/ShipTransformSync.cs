using QSB.SectorSync;
using QSB.Syncs.Sectored.Rigidbodies;
using QSB.Utility;
using UnityEngine;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : SectoredRigidbodySync
	{
		public static ShipTransformSync LocalInstance { get; private set; }

		private const int ForcePositionAfterUpdates = 50;
		private int _updateCount;

		protected override bool IsReady
			=> Locator.GetShipBody() != null;

		public override void Start()
		{
			base.Start();
			LocalInstance = this;
		}

		protected override OWRigidbody InitAttachedRigidbody()
		{
			SectorSync.Init(Locator.GetShipDetector().GetComponent<SectorDetector>(), TargetType.Ship);
			return Locator.GetShipBody();
		}

		protected override void ApplyToAttached()
		{
			ApplyToSector();
			// Dont do base... this is a replacement!

			if (ReferenceTransform == null || transform.position == Vector3.zero)
			{
				return;
			}

			var targetPos = ReferenceTransform.FromRelPos(transform.position);

			_updateCount++;
			if (_updateCount >= ForcePositionAfterUpdates)
			{
				_updateCount = 0;

				var targetRot = ReferenceTransform.FromRelRot(transform.rotation);

				AttachedRigidbody.SetPosition(targetPos);
				AttachedRigidbody.SetRotation(targetRot);
			}

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelAngVel(_relativeAngularVelocity);

			SetVelocity(AttachedRigidbody, targetVelocity);
			AttachedRigidbody.SetAngularVelocity(targetAngularVelocity);
		}

		/// use OWRigidbody version instead of ShipBody override
		private static void SetVelocity(OWRigidbody rigidbody, Vector3 newVelocity)
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

		protected override bool UseInterpolation => false;
	}
}
