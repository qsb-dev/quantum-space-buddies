using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System.Threading;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects;

public class QSBMeteor : WorldObject<MeteorController>
{
	private QSBMeteorLauncher _qsbMeteorLauncher;

	public override async UniTask Init(CancellationToken ct)
	{
		var meteorLauncher = AttachedObject._suspendRoot.GetComponent<MeteorLauncher>();
		await UniTask.WaitUntil(() => QSBWorldSync.AllObjectsAdded, cancellationToken: ct);
		_qsbMeteorLauncher = meteorLauncher.GetWorldObject<QSBMeteorLauncher>();
	}

	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public static bool IsSpecialImpact(GameObject go) =>
		go == Locator.GetPlayerCollider().gameObject || (Locator.GetProbe() != null && go == Locator.GetProbe()._anchor._collider.gameObject);

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
