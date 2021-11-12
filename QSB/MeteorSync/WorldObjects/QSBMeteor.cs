using System.Linq;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteor : WorldObject<MeteorController>
	{
		private bool _initialized;

		public override void Init(MeteorController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;

			// remove WorldObject if prefab
			if (AttachedObject.gameObject.scene.name == null)
			{
				QSBWorldSync.RemoveWorldObject(this);
				return;
			}

			_initialized = true;
		}

		public override void OnRemoval()
		{
			if (!_initialized)
			{
				return;
			}

			MeteorManager.MeteorsReady = false;
		}


		public float Damage;

		public void Impact(Vector3 impactPoint, float damage)
		{
			impactPoint = Locator._brittleHollow.transform.TransformPoint(impactPoint);
			Damage = damage;

			var hits = Physics.OverlapSphere(impactPoint, 1, OWLayerMask.physicalMask, QueryTriggerInteraction.Ignore);
			var obj = hits
				.Select(x => x.gameObject)
				.OrderBy(x => Vector3.Distance(impactPoint, x.transform.position))
				.FirstOrDefault();

			AttachedObject.owRigidbody.MoveToPosition(impactPoint);
			var impactVel = obj != null ? AttachedObject.owRigidbody.GetVelocity() - obj.GetAttachedOWRigidbody().GetVelocity() : default;
			AttachedObject.Impact(obj, impactPoint, impactVel);
		}
	}
}
