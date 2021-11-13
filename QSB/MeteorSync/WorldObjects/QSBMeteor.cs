using QSB.WorldSync;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteor : WorldObject<MeteorController>
	{
		public override void Init(MeteorController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
		}

		public override void OnRemoval()
		{
			MeteorManager.MeteorsReady = false;
		}


		public float Damage;
	}
}
