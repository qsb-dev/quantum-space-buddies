using System.Linq;
using OWML.Common;
using QSB.MeteorSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;
using Object = UnityEngine.Object;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteor : WorldObject<MeteorController>
	{
		public MeteorTransformSync TransformSync;
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

			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.Instance.MeteorPrefab).SpawnWithServerAuthority();
			}

			_initialized = true;
		}

		public override void OnRemoval()
		{
			if (!_initialized)
			{
				return;
			}

			if (QSBCore.IsHost)
			{
				QNetworkServer.Destroy(TransformSync.gameObject);
			}

			MeteorManager.MeteorsReady = false;
		}


		public float Damage = float.NaN;

		public void Impact(float damage)
		{
			Damage = damage;

			// just in case, set this up so even if no hit happens, it will reset itself eventually
			AttachedObject._hasImpacted = true;
			AttachedObject._impactTime = Time.time;

			// let the collision happen naturally
			foreach (var owCollider in AttachedObject._owColliders)
			{
				owCollider.SetActivation(true);
			}
		}
	}
}
