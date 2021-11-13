using QSB.WorldSync;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBMeteor : WorldObject<MeteorController>
	{
		private bool _initialized;

		public override void Init(MeteorController attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;

			// remove WorldObject if prefab
			if (AttachedObject.gameObject.scene.name == null)
			{
				QSBWorldSync.RemoveWorldObject(this);
				return;
			}

			_initialized = true;
		}

		public override void OnRemoval()
		{
			if (!_initialized)
			{
				return;
			}

			MeteorManager.MeteorsReady = false;
		}


		public float Damage;
	}
}
