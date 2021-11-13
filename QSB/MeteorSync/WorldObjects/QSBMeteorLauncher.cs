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


		public int MeteorId;
		public float LaunchSpeed;
		public float Damage;

		public void PreLaunchMeteor()
		{
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Play();
			}
		}

		public void LaunchMeteor(int meteorId, float launchSpeed, float damage)
		{
			MeteorId = meteorId;
			LaunchSpeed = launchSpeed;
			Damage = damage;

			AttachedObject.LaunchMeteor();
			foreach (var particleSystem in AttachedObject._launchParticles)
			{
				particleSystem.Stop();
			}
		}
	}
}
