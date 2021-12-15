using QSB.Utility;
using QSB.WorldSync;

namespace QSB.OrbSync.WorldObjects
{
	public class QSBOrbSlot : WorldObject<NomaiInterfaceSlot>
	{
		public void SetState(QSBOrb qsbOrb, bool state)
		{
			var orb = state ? qsbOrb.AttachedObject : null;
			if (orb == AttachedObject._occupyingOrb) {
				return;
			}

			AttachedObject._occupyingOrb = orb;
			var ev = state ? nameof(AttachedObject.OnSlotActivated) : nameof(AttachedObject.OnSlotDeactivated);
			AttachedObject.RaiseEvent(ev, AttachedObject);
		}
	}
}
