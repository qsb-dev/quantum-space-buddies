using QSB.SectorSync;
using QSB.Syncs.Sectored.Rigidbodies;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ShipSync.TransformSync
{
	public class ShipTransformSync : SectoredRigidbodySync
	{
		public static ShipTransformSync LocalInstance { get; private set; }

		public override bool IsPlayerObject => false;

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
			QSBCore.UnityEvents.RunWhen(() => WorldObjectManager.AllObjectsReady, () => SectorSync.Init(Locator.GetShipDetector().GetComponent<SectorDetector>(), Locator.GetShipBody(), TargetType.Ship));
			return Locator.GetShipBody();
		}

		private void ForcePosition()
		{
			if (ReferenceTransform == null || transform.position == Vector3.zero)
			{
				return;
			}

			var targetPos = ReferenceTransform.FromRelPos(transform.position);
			var targetRot = ReferenceTransform.FromRelRot(transform.rotation);

			AttachedObject.SetPosition(targetPos);
			AttachedObject.SetRotation(targetRot);
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

			if (ReferenceTransform == null || transform.position == Vector3.zero)
			{
				return false;
			}

			var targetPos = ReferenceTransform.FromRelPos(transform.position);

			var targetVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelVel(_relativeVelocity, targetPos);
			var targetAngularVelocity = ReferenceTransform.GetAttachedOWRigidbody().FromRelAngVel(_relativeAngularVelocity);

			SetVelocity(AttachedObject, targetVelocity);
			AttachedObject.SetAngularVelocity(targetAngularVelocity);

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

		public override bool UseInterpolation => false;
	}
}
