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


		public void LaunchMeteor(bool preLaunch)
		{
			if (preLaunch)
			{
				DebugLog.DebugWrite($"{LogName} - pre launch");

				foreach (var particleSystem in AttachedObject._launchParticles)
				{
					particleSystem.Play();
				}
			}
			else
			{
				DebugLog.DebugWrite($"{LogName} - launch");

				AttachedObject.LaunchMeteor();
				foreach (var particleSystem in AttachedObject._launchParticles)
				{
					particleSystem.Stop();
				}
			}
		}
	}
}
