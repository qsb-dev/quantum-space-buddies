using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.OrbSync.WorldObjects
{
	public class QSBOrbSlot : WorldObject<NomaiInterfaceSlot>
	{
		public void HandleEvent(bool state, int orbId)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return;
			}

			QSBEventManager.FireEvent(EventNames.QSBOrbSlot, ObjectId, orbId, state);
		}

		public void SetState(bool state, int orbId)
		{
			var occOrb = state ? OrbManager.Orbs[orbId] : null;
			AttachedObject._occupyingOrb = occOrb;
			var ev = state ? "OnSlotActivated" : "OnSlotDeactivated";
			AttachedObject.RaiseEvent(ev, AttachedObject);
		}
	}
}
