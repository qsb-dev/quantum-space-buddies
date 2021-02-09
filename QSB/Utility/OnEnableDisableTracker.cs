using System;
using UnityEngine;

namespace QSB.Utility
{
	public class OnEnableDisableTracker : MonoBehaviour
	{
		public event Action OnEnableEvent;
		public event Action OnDisableEvent;

		public MonoBehaviour AttachedComponent;

		private ComponentState _wasEnabled = ComponentState.NotChecked;

		public OnEnableDisableTracker()
			=> QSBSceneManager.OnSceneLoaded += (OWScene scene, bool inUniverse) => Destroy(this);

		private void OnDestroy()
			=> QSBSceneManager.OnSceneLoaded -= (OWScene scene, bool inUniverse) => Destroy(this);

		private void Update()
		{
			if (AttachedComponent == null)
			{
				DebugLog.ToConsole($"Attached component is null!", OWML.Common.MessageType.Error);
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