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


		public bool Flag;
		public int PoolIndex;
		public float LaunchSpeed;

		public void PreLaunchMeteor()
		{
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Play();
			}

			DebugLog.DebugWrite($"{LogName} - pre launch");
		}

		public void LaunchMeteor(bool flag, int poolIndex, float launchSpeed)
		{
			Flag = flag;
			PoolIndex = poolIndex;
			LaunchSpeed = launchSpeed;

			AttachedObject.LaunchMeteor();
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Stop();
			}

			DebugLog.DebugWrite($"{LogName} - launch {flag} {poolIndex} {launchSpeed}");
		}
	}
}
