using OWML.Utils;
using QSB.Events;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine.UI;

namespace QSB.OrbSync.WorldObjects
{
	public class QSBOrbSlot : WorldObject<NomaiInterfaceSlot>
	{
		public bool Activated { get; private set; }

		private bool _initialized;
		private Text _debugBoxText;

		public override void OnRemoval()
			=> UnityEngine.Object.Destroy(_debugBoxText.gameObject);

		public override void Init(NomaiInterfaceSlot slot, int id)
		{
			ObjectId = id;
			AttachedObject = slot;
			_initialized = true;
			if (QSBCore.DebugMode)
			{
				_debugBoxText = DebugBoxManager.CreateBox(AttachedObject.transform, 0, AttachedObject.IsActivated().ToString()).GetComponent<Text>();
			}
		}

		public void HandleEvent(bool state, int orbId)
		{
			if (!QSBCore.HasWokenUp)
			{
				return;
			}
			QSBEventManager.FireEvent(EventNames.QSBOrbSlot, ObjectId, orbId, state);
			if (QSBCore.DebugMode)
			{
				_debugBoxText.text = state.ToString();
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
			if (QSBCore.DebugMode)
			{
				_debugBoxText.text = state.ToString();
			}
		}
	}
}