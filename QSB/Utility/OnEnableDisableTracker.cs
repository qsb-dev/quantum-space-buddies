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

		public OnEnableDisableTracker()
		{
			var hook = QSBCore.GameObjectInstance.AddComponent<UpdateDisabledHook>();
			hook.Component = this;
		}

		public void DoUpdate()
		{
			if (AttachedComponent == null)
			{
				DebugLog.DebugWrite($"Attached component is null! Attached to {gameObject.name}", OWML.Common.MessageType.Error);
				return;
			}
			var state = AttachedComponent.isActiveAndEnabled ? ComponentState.Enabled : ComponentState.Disabled;
			if (_wasEnabled != state)
			{
				_wasEnabled = state;
				if (state == ComponentState.Enabled)
				{
					OnEnableEvent?.Invoke();
				}
				else
				{
					OnDisableEvent?.Invoke();
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