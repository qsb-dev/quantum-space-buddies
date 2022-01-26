using QSB.Messaging;
using QSB.ShipSync.Messages.Hull;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ShipSync.WorldObjects
{
	internal class QSBShipHull : WorldObject<ShipHull>
	{
		public override void SendInitialState(uint to)
		{
			if (QSBCore.IsHost)
			{
				if (AttachedObject._damaged)
				{
					this.SendMessage(new HullDamagedMessage());
				}
				else
				{
					this.SendMessage(new HullRepairedMessage());
				}

				this.SendMessage(new HullChangeIntegrityMessage(AttachedObject._integrity));
			}
		}

		public void SetDamaged()
		{
			if (AttachedObject._damaged)
			{
				return;
			}

			DebugLog.DebugWrite($"[HULL] {AttachedObject} Set damaged.");
			AttachedObject._damaged = true;
			AttachedObject.RaiseEvent(nameof(AttachedObject.OnDamaged), AttachedObject);
		}

		public void SetRepaired()
		{
			if (!AttachedObject._damaged)
			{
				return;
			}

			DebugLog.DebugWrite($"[HULL] {AttachedObject} Set repaired.");
			AttachedObject._damaged = false;
			AttachedObject.RaiseEvent(nameof(AttachedObject.OnRepaired), AttachedObject);
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(0f);
		}

		public void ChangeIntegrity(float newIntegrity)
		{
			if (OWMath.ApproxEquals(AttachedObject._integrity, newIntegrity))
			{
				return;
			}

			DebugLog.DebugWrite($"[HULL] {AttachedObject} Change integrity to {newIntegrity}.");
			AttachedObject._integrity = newIntegrity;
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(1f - newIntegrity);
		}
	}
}
