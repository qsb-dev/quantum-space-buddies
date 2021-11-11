using QSB.Player;
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
				DebugLog.DebugWrite($"{QSBPlayerManager.LocalPlayerId} {ObjectId} - pre launch");

				foreach (var particleSystem in AttachedObject._launchParticles)
				{
					particleSystem.Play();
				}
			}
			else
			{
				DebugLog.DebugWrite($"{QSBPlayerManager.LocalPlayerId} {ObjectId} - launch");

				AttachedObject.LaunchMeteor();
				foreach (var particleSystem in AttachedObject._launchParticles)
				{
					particleSystem.Stop();
				}
			}
		}
	}
}
