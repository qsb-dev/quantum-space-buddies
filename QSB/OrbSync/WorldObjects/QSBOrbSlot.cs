using QSB.Utility;
using QSB.WorldSync;

namespace QSB.OrbSync.WorldObjects
{
	public class QSBOrbSlot : WorldObject<NomaiInterfaceSlot>
	{
		public void SetState(QSBOrb qsbOrb, bool state)
		{
			AttachedObject._occupyingOrb = state ? qsbOrb.AttachedObject : null;
			var ev = state ? nameof(AttachedObject.OnSlotActivated) : nameof(AttachedObject.OnSlotDeactivated);
			AttachedObject.RaiseEvent(ev, AttachedObject);
		}
	}
}
