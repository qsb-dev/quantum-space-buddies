using OWML.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.Utility
{
	public class OnEnableDisableTracker : MonoBehaviour, IRepeating
	{
		public event Action OnEnableEvent;
		public event Action OnDisableEvent;

		public MonoBehaviour AttachedComponent
		{
			get => _attachedComponent;
			set
			{
				_attachedComponent = value;
				_visibilityTrackers = _attachedComponent.GetValue<VisibilityTracker[]>("_visibilityTrackers");
			}
		}

		private MonoBehaviour _attachedComponent;
		private VisibilityTracker[] _visibilityTrackers;

		private ComponentState _wasEnabled = ComponentState.NotChecked;

		public OnEnableDisableTracker()
		{
			RepeatingManager.Repeatings.Add(this);
			QSBSceneManager.OnSceneLoaded += (OWScene scene, bool inUniverse) => Destroy(this);
		}

		private void OnDestroy()
		{
			RepeatingManager.Repeatings.Remove(this);
			QSBSceneManager.OnSceneLoaded -= (OWScene scene, bool inUniverse) => Destroy(this);
		}

		private bool GetAnyVisibilityTrackersActive() 
			=> _visibilityTrackers.All(x => x.GetValue<Shape[]>("_shapes").All(y => y.enabled));

		public void Invoke()
		{
			if (AttachedComponent == null)
			{
				DebugLog.ToConsole($"Attached component is null!", OWML.Common.MessageType.Error);
				return;
			}
			var state = AttachedComponent.isActiveAndEnabled && GetAnyVisibilityTrackersActive() ? ComponentState.Enabled : ComponentState.Disabled;
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