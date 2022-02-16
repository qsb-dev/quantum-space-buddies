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

		protected override bool CheckReady() => base.CheckReady()
			&& Locator.GetShipBody();

		public override void OnStartClient()
		{
			base.OnStartClient();
			LocalInstance = this;
		}

		protected override OWRigidbody InitAttachedRigidbody()
		{
			SectorDetector.Init(Locator.GetShipDetector().GetComponent<SectorDetector>());
			return Locator.GetShipBody();
		}

		/// Dont do base... this is a replacement!
		protected override void ApplyToAttached()
		{
			ApplyToSector();
			if (!ReferenceTransform)
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

			var targetVelocity = ReferenceRigidbody.FromRelVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceRigidbody.FromRelAngVel(_relativeAngularVelocity);

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
