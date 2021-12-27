using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ShipSync.WorldObjects
{
	internal class QSBShipHull : WorldObject<ShipHull>
	{
		public void SetDamaged()
		{
			DebugLog.DebugWrite($"[HULL] {AttachedObject} Set damaged.");
			AttachedObject._damaged = true;
			AttachedObject.RaiseEvent(nameof(AttachedObject.OnDamaged), AttachedObject);
		}

		public void SetRepaired()
		{
			DebugLog.DebugWrite($"[HULL] {AttachedObject} Set repaired.");
			AttachedObject._damaged = false;
			AttachedObject.RaiseEvent(nameof(AttachedObject.OnRepaired), AttachedObject);
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(0f);
		}

		public void ChangeIntegrity(float newIntegrity)
		{
			DebugLog.DebugWrite($"[HULL] {AttachedObject} Change integrity to {newIntegrity}.");
			AttachedObject._integrity = newIntegrity;
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(1f - newIntegrity);
		}

		public void RepairTick(float integrity)
		{
			AttachedObject._integrity = integrity;
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(1f - integrity);
		}
	}
}
