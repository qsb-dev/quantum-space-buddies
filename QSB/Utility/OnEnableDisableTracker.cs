using UnityEngine;

namespace QSB.Utility
{
	public delegate void EnableDisableEvent();

	public class OnEnableDisableTracker : MonoBehaviour
	{
		public event EnableDisableEvent OnEnableEvent;
		public event EnableDisableEvent OnDisableEvent;

		public OnEnableDisableTracker()
		{
			if (gameObject.activeInHierarchy)
			{
				OnEnableEvent?.Invoke();
			}
			else
			{
				OnDisableEvent?.Invoke();
			}
		}

		private void OnEnable() => OnEnableEvent?.Invoke();
		private void OnDisable() => OnDisableEvent?.Invoke();
	}
}