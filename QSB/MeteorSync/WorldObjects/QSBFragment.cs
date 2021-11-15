using QSB.WorldSync;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBFragment : WorldObject<FragmentIntegrity>
	{
		public override void Init(FragmentIntegrity attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			DetachableFragment = AttachedObject.GetRequiredComponent<DetachableFragment>();
		}

		public override void OnRemoval()
		{
			MeteorManager.Ready = false;
		}


		public DetachableFragment DetachableFragment;
		public bool IsThruWhiteHole => DetachableFragment._sector._parentSector == MeteorManager.WhiteHoleVolume._whiteHoleSector;

		public float LeashLength
		{
			get => AttachedObject.GetComponent<DebrisLeash>()._leashLength;
			set
			{
				var debrisLeash = AttachedObject.GetComponent<DebrisLeash>();
				debrisLeash._deccelerating = false;
				debrisLeash._leashLength = value;
			}
		}

		public void AddDamage(float damage)
		{
			AttachedObject.AddDamage(damage);
		}
	}
}
