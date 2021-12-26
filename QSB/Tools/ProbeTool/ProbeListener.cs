using QSB.Messaging;
using QSB.Tools.ProbeTool.Messages;
using UnityEngine;

namespace QSB.Tools.ProbeTool
{
	internal class ProbeListener : MonoBehaviour
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

		private static void OnLaunchProbe()
			=> new PlayerProbeMessage(ProbeEvent.Launch).Send();

		private static void OnAnchorProbe()
			=> new PlayerProbeMessage(ProbeEvent.Anchor).Send();

		private static void OnUnanchorProbe()
			=> new PlayerProbeMessage(ProbeEvent.Unanchor).Send();

		private static void OnRetrieveProbe()
			=> new PlayerProbeMessage(ProbeEvent.Retrieve).Send();

		private static void OnProbeDestroyed()
			=> new PlayerProbeMessage(ProbeEvent.Destroy).Send();

		private static void OnStartRetrieveProbe(float length)
			=> new ProbeStartRetrieveMessage(length).Send();
	}
}
