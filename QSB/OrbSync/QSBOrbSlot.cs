using OWML.Utils;
using QSB.Events;
using QSB.WorldSync;

namespace QSB.OrbSync
{
	public class QSBOrbSlot : WorldObject<NomaiInterfaceSlot>
	{
		public bool Activated { get; private set; }

		private bool _initialized;

		public override void Init(NomaiInterfaceSlot slot, int id)
		{
			ObjectId = id;
			AttachedObject = slot;
			_initialized = true;
			QSBWorldSync.AddWorldObject(this);
		}

		public void HandleEvent(bool state, int orbId)
		{
			if (QSBCore.HasWokenUp)
			{
				GlobalMessenger<int, int, bool>.FireEvent(EventNames.QSBOrbSlot, ObjectId, orbId, state);
			}
		}

		public void SetState(bool state, int orbId)
		{
			if (!_initialized)
			{
				return;
			}
			var occOrb = state ? QSBWorldSync.OldOrbList[orbId] : null;
			AttachedObject.SetValue("_occupyingOrb", occOrb);
			var ev = state ? "OnSlotActivated" : "OnSlotDeactivated";
			QSBWorldSync.RaiseEvent(AttachedObject, ev);
			Activated = state;
		}
	}
}