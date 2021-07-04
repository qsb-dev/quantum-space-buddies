using QSB.Events;
using QSB.Utility;
using UnityEngine;

namespace QSB.ProbeSync
{
	class ProbeListener : MonoBehaviour
	{
		private SurveyorProbe _attachedProbe;

		public void Init(SurveyorProbe localProbe)
		{
			_attachedProbe = localProbe;
			_attachedProbe.OnLaunchProbe += OnLaunchProbe;
			_attachedProbe.OnAnchorProbe += OnAnchorProbe;
			_attachedProbe.OnUnanchorProbe += OnUnanchorProbe;
			_attachedProbe.OnRetrieveProbe += OnRetrieveProbe;
			_attachedProbe.OnProbeDestroyed += OnProbeDestroyed;
			_attachedProbe.OnStartRetrieveProbe += OnStartRetrieveProbe;
		}

		private void OnDestroy()
		{
			if (_attachedProbe == null)
			{
				return;
			}

			_attachedProbe.OnLaunchProbe -= OnLaunchProbe;
			_attachedProbe.OnAnchorProbe -= OnAnchorProbe;
			_attachedProbe.OnUnanchorProbe -= OnUnanchorProbe;
			_attachedProbe.OnRetrieveProbe -= OnRetrieveProbe;
			_attachedProbe.OnProbeDestroyed -= OnProbeDestroyed;
			_attachedProbe.OnStartRetrieveProbe -= OnStartRetrieveProbe;
		}

		private void OnLaunchProbe()
		{
			DebugLog.DebugWrite($"LOCAL OnLaunchProbe");
			QSBEventManager.FireEvent(EventNames.QSBProbeEvent, ProbeEvent.Launch);
		}

		private void OnAnchorProbe()
		{
			DebugLog.DebugWrite($"LOCAL OnAnchorProbe");
			QSBEventManager.FireEvent(EventNames.QSBProbeEvent, ProbeEvent.Anchor);
		}

		private void OnUnanchorProbe()
		{
			DebugLog.DebugWrite($"LOCAL OnUnanchorProbe");
			QSBEventManager.FireEvent(EventNames.QSBProbeEvent, ProbeEvent.Unanchor);
		}

		private void OnRetrieveProbe()
		{
			DebugLog.DebugWrite($"LOCAL OnRetrieveProbe");
			QSBEventManager.FireEvent(EventNames.QSBProbeEvent, ProbeEvent.Retrieve);
		}

		private void OnProbeDestroyed()
		{
			DebugLog.DebugWrite($"LOCAL OnProbeDestroyed");
			QSBEventManager.FireEvent(EventNames.QSBProbeEvent, ProbeEvent.Destroy);
		}

		private void OnStartRetrieveProbe(float length)
		{
			DebugLog.DebugWrite($"LOCAL OnStartRetrieveProbe length:{length}");
		}
	}
}
