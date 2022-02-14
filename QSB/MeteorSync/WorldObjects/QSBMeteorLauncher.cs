using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteorLauncher : WorldObject<MeteorLauncher>
	{
		public override void SendInitialState(uint to)
		{
			// todo SendInitialState
		}

		public void PreLaunchMeteor()
		{
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Play();
			}
		}

		public void LaunchMeteor(QSBMeteor qsbMeteor, float launchSpeed)
		{
			var meteorController = qsbMeteor.AttachedObject;
			if (meteorController.hasLaunched)
			{
				DebugLog.DebugWrite($"{qsbMeteor} of {this} has already launched", MessageType.Warning);
				return;
			}

			var linearVelocity = AttachedObject._parentBody.GetPointVelocity(AttachedObject.transform.position) + AttachedObject.transform.TransformDirection(AttachedObject._launchDirection) * launchSpeed;
			var angularVelocity = AttachedObject.transform.forward * 2f;
			meteorController.Launch(null, AttachedObject.transform.position, AttachedObject.transform.rotation, linearVelocity, angularVelocity);
			if (AttachedObject._audioSector.ContainsOccupant(DynamicOccupant.Player))
			{
				AttachedObject._launchSource.pitch = Random.Range(0.4f, 0.6f);
				AttachedObject._launchSource.PlayOneShot(AudioType.BH_MeteorLaunch);
			}

			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Stop();
			}
		}
	}
}
