using UnityEngine;

namespace QSB.Utility
{
	public delegate void EnableDisableEvent();

	public class OnEnableDisableTracker : MonoBehaviour
	{
		public event EnableDisableEvent OnEnableEvent;
		public event EnableDisableEvent OnDisableEvent;

		public MonoBehaviour AttachedComponent;

		private ComponentState _wasEnabled = ComponentState.NotChecked;

		private void Update()
		{
			if (AttachedComponent == null)
			{
				DebugLog.ToConsole($"Attached component is null! Attached to {gameObject.name}", OWML.Common.MessageType.Error);
				return;
			}
			var state = AttachedComponent.isActiveAndEnabled ? ComponentState.Enabled : ComponentState.Disabled;
			if (_wasEnabled != state)
			{
				_wasEnabled = state;
				if (state == ComponentState.Enabled)
				{
					OnEnableEvent?.SafeInvoke();
				}
				else
				{
					OnDisableEvent?.SafeInvoke();
				}
			}
		}
	}

	internal enum ComponentState
	{
		NotChecked = 0,
		Enabled = 1,
		Disabled = 2
	}
}