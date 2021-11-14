using QSB.Utility;
using QSB.WorldSync;

namespace QSB.MeteorSync.WorldObjects
{
	public class QSBFragment : WorldObject<FragmentIntegrity>
	{
		public override void Init(FragmentIntegrity attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
		}

		public void AddDamage(float damage)
		{
			AttachedObject.AddDamage(damage);
			DebugLog.DebugWrite($"{LogName} - damage {damage}");
		}
	}
}
