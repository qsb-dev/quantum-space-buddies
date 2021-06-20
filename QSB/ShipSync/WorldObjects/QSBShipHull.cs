using OWML.Utils;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ShipSync.WorldObjects
{
	class QSBShipHull : WorldObject<ShipHull>
	{
		public override void Init(ShipHull hull, int id)
		{
			ObjectId = id;
			AttachedObject = hull;
		}

		public void SetDamaged()
		{
			DebugLog.DebugWrite($"[HULL] {AttachedObject} Set damaged.");
			AttachedObject.SetValue("_damaged", true);
			QSBWorldSync.RaiseEvent(AttachedObject, "OnDamaged", AttachedObject);
		}

		public void ChangeIntegrity(float newIntegrity)
		{
			DebugLog.DebugWrite($"[HULL] {AttachedObject} Change integrity to {newIntegrity}.");
			AttachedObject.SetValue("_integrity", newIntegrity);
			var damageEffect = AttachedObject.GetValue<DamageEffect>("_damageEffect");
			damageEffect.SetEffectBlend(1f - newIntegrity);
		}
	}
}
