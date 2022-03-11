using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects;

public class QSBMeteorLauncher : WorldObject<MeteorLauncher>
{
	private QSBMeteor[] _qsbMeteors;

	public override async UniTask Init(CancellationToken ct)
	{
		var meteors = AttachedObject._meteorPool.Concat(AttachedObject._dynamicMeteorPool);
		await UniTask.WaitUntil(() => QSBWorldSync.AllObjectsAdded, cancellationToken: ct);
		_qsbMeteors = meteors.Select(x => x.GetWorldObject<QSBMeteor>()).ToArray();
	}

	public override void SendInitialState(uint to)
	{
		// todo SendInitialState
	}

	public void PreLaunchMeteor()
	{
		foreach (var launchParticle in AttachedObject._launchParticles)
		{
			launchParticle.Play();
		}
	}

	public void LaunchMeteor(MeteorController meteorController, float launchSpeed)
	{
		meteorController.Initialize(AttachedObject.transform, AttachedObject._detectableField, AttachedObject._detectableFluid);

		var linearVelocity = AttachedObject._parentBody.GetPointVelocity(AttachedObject.transform.position) + AttachedObject.transform.TransformDirection(AttachedObject._launchDirection) * launchSpeed;
		var angularVelocity = AttachedObject.transform.forward * 2f;
		meteorController.Launch(null, AttachedObject.transform.position, AttachedObject.transform.rotation, linearVelocity, angularVelocity);
		if (AttachedObject._audioSector.ContainsOccupant(DynamicOccupant.Player))
		{
			AttachedObject._launchSource.pitch = Random.Range(0.4f, 0.6f);
			AttachedObject._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
		}

		foreach (var launchParticle in AttachedObject._launchParticles)
		{
			launchParticle.Stop();
		}
	}
}
