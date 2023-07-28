using QSB.Messaging;
using QSB.ShipSync.Messages.Component;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ShipSync.WorldObjects;

public class QSBShipComponent : WorldObject<ShipComponent>
{
	public override void SendInitialState(uint to)
	{
		if (AttachedObject._damaged)
		{
			this.SendMessage(new ComponentDamagedMessage { To = to });
		}
		else
		{
			this.SendMessage(new ComponentRepairedMessage { To = to });
		}

		this.SendMessage(new ComponentRepairTickMessage(AttachedObject._repairFraction) { To = to });
	}

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