using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBFragment : WorldObject<FragmentIntegrity>
	{
		public override void Init(FragmentIntegrity attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			DetachableFragment = AttachedObject.GetComponent<DetachableFragment>();

			if (QSBCore.IsHost)
			{
				LeashLength = Random.Range(MeteorManager.WhiteHoleVolume._debrisDistMin, MeteorManager.WhiteHoleVolume._debrisDistMax);
			}
		}


		public DetachableFragment DetachableFragment;
		public bool IsThruWhiteHole => DetachableFragment != null &&
			DetachableFragment._sector._parentSector == MeteorManager.WhiteHoleVolume._whiteHoleSector;
		public OWRigidbody RefBody => IsThruWhiteHole ? MeteorManager.WhiteHoleVolume._whiteHoleBody : Locator._brittleHollow._owRigidbody;
		public OWRigidbody Body
		{
			get
			{
				if (IsThruWhiteHole)
				{
					return AttachedObject.transform.parent.parent.GetAttachedOWRigidbody();
				}
				DebugLog.ToConsole($"{LogName} - trying to get rigidbody when not thru white hole. "
					+ "did you mean to get the transform instead?", MessageType.Error);
				return null;
			}
		}

		/// what the leash length will be when we eventually detach and fall thru white hole
		public float LeashLength;

		public void AddDamage(float damage)
		{
			AttachedObject.AddDamage(damage);
		}
	}
}
