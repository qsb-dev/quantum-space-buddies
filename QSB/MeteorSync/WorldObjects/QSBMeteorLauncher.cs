using QSB.Utility;
using QSB.WorldSync;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteorLauncher : WorldObject<MeteorLauncher>
	{
		public override void Init(MeteorLauncher attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
		}


		public float LaunchSpeed;

		public void PreLaunchMeteor()
		{
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Play();
			}

			DebugLog.DebugWrite($"{LogName} - pre launch");
		}

		public void LaunchMeteor(float launchSpeed)
		{
			LaunchSpeed = launchSpeed;

			AttachedObject.LaunchMeteor();
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Stop();
			}

			DebugLog.DebugWrite($"{LogName} - launch {launchSpeed}");
		}
	}
}
