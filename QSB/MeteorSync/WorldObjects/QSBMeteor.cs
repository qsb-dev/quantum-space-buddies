using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects;

public class QSBMeteor : WorldObject<MeteorController>
{
	public override void SendInitialState(uint to)
	{
		// we don't really need to sync initial state
	}

	public static bool IsSpecialImpact(GameObject go) =>
		go == Locator.GetPlayerCollider().gameObject ||
		Locator.GetProbe() != null && go == Locator.GetProbe()._anchor._collider.gameObject;

	public void SpecialImpact()
	{
		AttachedObject._intactRenderer.enabled = false;
		AttachedObject._impactLight.enabled = true;
		AttachedObject._impactLight.intensity = AttachedObject._impactLightCurve.Evaluate(0f);
		foreach (var impactParticle in AttachedObject._impactParticles)
		{
			impactParticle.Play();
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
