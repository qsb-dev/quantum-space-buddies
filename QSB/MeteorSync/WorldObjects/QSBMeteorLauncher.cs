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
				foreach (var particleSystem in AttachedObject._launchParticles)
				{
					particleSystem.Play();
				}
			}
			else
			{
				AttachedObject.LaunchMeteor();
				foreach (var particleSystem in AttachedObject._launchParticles)
				{
					particleSystem.Stop();
				}
			}
		}
	}
}
