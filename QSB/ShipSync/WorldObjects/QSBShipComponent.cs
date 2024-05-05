using QSB.Utility;
﻿using OWML.Utils;
using QSB.WorldSync;

namespace QSB.ShipSync.WorldObjects;

public class QSBShipComponent : WorldObject<ShipComponent>
{
	public void SetDamaged()
	{
		if (AttachedObject._damaged)
		{
			return;
		}

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
		if (!AttachedObject._damaged)
		{
			return;
		}

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
		if (OWMath.ApproxEquals(AttachedObject._repairFraction, repairFraction))
		{
			return;
		}

		AttachedObject._repairFraction = repairFraction;
		var damageEffect = AttachedObject._damageEffect;
		damageEffect.SetEffectBlend(1f - repairFraction);
	}
}