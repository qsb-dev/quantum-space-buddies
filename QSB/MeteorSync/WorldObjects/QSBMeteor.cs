using QSB.Utility;
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


		public bool ShouldImpact;
		public float Damage;

		public void Impact(Vector3 pos, Quaternion rot, float damage)
		{
			pos = Locator._brittleHollow.transform.TransformPoint(pos);
			rot = Locator._brittleHollow.transform.TransformRotation(rot);
			Damage = damage;

			AttachedObject.owRigidbody.SetPosition(pos);
			AttachedObject.owRigidbody.SetRotation(rot);

			foreach (var owCollider in AttachedObject._owColliders)
			{
				owCollider.SetActivation(!OWLayerMask.IsLayerInMask(owCollider.gameObject.layer, OWLayerMask.physicalMask));
			}
			FragmentSurfaceProxy.TrackMeteor(AttachedObject);
			FragmentCollisionProxy.TrackMeteor(AttachedObject);

			AttachedObject._hasImpacted = true;
			AttachedObject._impactTime = Time.time;

			ShouldImpact = true;
		}
	}
}
