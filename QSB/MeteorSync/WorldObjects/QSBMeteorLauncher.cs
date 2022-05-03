using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects;

public class QSBMeteorLauncher : WorldObject<MeteorLauncher>
{
	public override void SendInitialState(uint to)
	{
		// we don't really need to sync initial state
	}

	public void PreLaunchMeteor()
	{
		foreach (var launchParticle in AttachedObject._launchParticles)
		{
			launchParticle.Play();
		}
	}

	public void LaunchMeteor(MeteorController meteor, float launchSpeed)
	{
		meteor.Initialize(AttachedObject.transform, AttachedObject._detectableField, AttachedObject._detectableFluid);

		var linearVelocity = AttachedObject._parentBody.GetPointVelocity(AttachedObject.transform.position) + AttachedObject.transform.TransformDirection(AttachedObject._launchDirection) * launchSpeed;
		var angularVelocity = AttachedObject.transform.forward * 2f;
		meteor.Launch(null, AttachedObject.transform.position, AttachedObject.transform.rotation, linearVelocity, angularVelocity);
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
