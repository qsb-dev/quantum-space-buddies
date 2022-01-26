using QSB.WorldSync;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteorLauncher : WorldObject<MeteorLauncher>
	{
		public override void SendInitialState(uint to)
		{
			// todo SendInitialState
		}

		public int MeteorId;
		public float LaunchSpeed;

		public void PreLaunchMeteor()
		{
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Play();
			}
		}

		public void LaunchMeteor(int meteorId, float launchSpeed)
		{
			MeteorId = meteorId;
			LaunchSpeed = launchSpeed;

			AttachedObject.LaunchMeteor();
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Stop();
			}
		}
	}
}
