using QSB.Messaging;
using QSB.Player;
using QSB.Tools.ProbeTool.Messages;
using UnityEngine;

namespace QSB.Tools.ProbeTool;

public class ProbeListener : MonoBehaviour
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
	{
		QSBPlayerManager.LocalPlayer.ProbeActive = true;
		new PlayerProbeEventMessage(ProbeEvent.Launch).Send();
	}

	private static void OnAnchorProbe()
	{
		QSBPlayerManager.LocalPlayer.ProbeActive = true;
		new PlayerProbeEventMessage(ProbeEvent.Anchor).Send();
	}

	private static void OnUnanchorProbe()
	{
		QSBPlayerManager.LocalPlayer.ProbeActive = true;
		new PlayerProbeEventMessage(ProbeEvent.Unanchor).Send();
	}

	private static void OnRetrieveProbe()
	{
		QSBPlayerManager.LocalPlayer.ProbeActive = false;
		new PlayerProbeEventMessage(ProbeEvent.Retrieve).Send();
	}

	private static void OnProbeDestroyed()
	{
		QSBPlayerManager.LocalPlayer.ProbeActive = false;
		new PlayerProbeEventMessage(ProbeEvent.Destroy).Send();
	}

	private static void OnStartRetrieveProbe(float length)
		=> new ProbeStartRetrieveMessage(length).Send();
}