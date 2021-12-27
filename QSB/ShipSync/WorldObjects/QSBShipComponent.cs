using OWML.Utils;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ShipSync.WorldObjects
{
	internal class QSBShipComponent : WorldObject<ShipComponent>
	{
		public void SetDamaged()
		{
			DebugLog.DebugWrite($"[S COMPONENT] {AttachedObject} Set damaged.");
			AttachedObject._damaged = true;
			AttachedObject._repairFraction = 0f;
			AttachedObject.OnComponentDamaged();
			AttachedObject.RaiseEvent(nameof(AttachedObject.OnDamaged), AttachedObject);
			AttachedObject.UpdateColliderState();
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(1f);
		}

		public void SetRepaired()
		{
			DebugLog.DebugWrite($"[S COMPONENT] {AttachedObject} Set repaired.");
			AttachedObject._damaged = false;
			AttachedObject._repairFraction = 1f;
			AttachedObject.OnComponentRepaired();
			AttachedObject.RaiseEvent(nameof(AttachedObject.OnRepaired), AttachedObject);
			AttachedObject.UpdateColliderState();
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(0f);
		}

		public void RepairTick(float repairFraction)
		{
			AttachedObject._repairFraction = repairFraction;
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(1f - repairFraction);
		}
	}
}
