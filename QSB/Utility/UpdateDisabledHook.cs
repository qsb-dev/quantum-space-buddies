using UnityEngine;

namespace QSB.Utility
{
	internal class UpdateDisabledHook : MonoBehaviour
	{
		public OnEnableDisableTracker Component;

		private void Update() => Component.DoUpdate();
	}
}