using QSB.MeteorSync.TransformSync;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteor : WorldObject<MeteorController>
	{
		public MeteorTransformSync TransformSync;

		public override void Init(MeteorController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;

			if (QSBCore.IsHost)
			{
				Object.Instantiate(QSBNetworkManager.Instance.MeteorPrefab).SpawnWithServerAuthority();
			}
		}

		public override void OnRemoval()
		{
			if (QSBCore.IsHost)
			{
				QNetworkServer.Destroy(TransformSync.gameObject);
			}

			MeteorManager.Ready = false;
		}


		public static bool IsSpecialImpact(GameObject go) =>
			go == Locator.GetPlayerCollider().gameObject || go == Locator.GetProbe()._anchor._collider.gameObject;

		public void SpecialImpact()
		{
			AttachedObject._intactRenderer.enabled = false;
			AttachedObject._impactLight.enabled = true;
			AttachedObject._impactLight.intensity = AttachedObject._impactLightCurve.Evaluate(0f);
			foreach (var particleSystem in AttachedObject._impactParticles)
			{
				particleSystem.Play();
			}
			AttachedObject._impactSource.PlayOneShot(AudioType.BH_MeteorImpact);
			foreach (var owCollider in AttachedObject._owColliders)
			{
				owCollider.SetActivation(false);
			}
			AttachedObject._owRigidbody.MakeKinematic();
			FragmentSurfaceProxy.UntrackMeteor(AttachedObject);
			FragmentCollisionProxy.UntrackMeteor(AttachedObject);
			AttachedObject._ignoringCollisions = false;
			AttachedObject._hasImpacted = true;
			AttachedObject._impactTime = Time.time;
		}
	}
}
