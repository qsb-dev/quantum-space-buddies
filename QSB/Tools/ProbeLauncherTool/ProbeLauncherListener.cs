using QSB.Messaging;
using QSB.Tools.ProbeLauncherTool.Messages;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool;

public class ProbeLauncherListener : MonoBehaviour
{
	private PlayerProbeLauncher _attachedLauncher;

	public void Init(PlayerProbeLauncher localLauncher)
	{
		_attachedLauncher = localLauncher;
		_attachedLauncher.OnLaunchProbe += OnLaunchProbe;
	}

	private void OnDestroy() =>
		_attachedLauncher.OnLaunchProbe -= OnLaunchProbe;

	private static void OnLaunchProbe(SurveyorProbe probe) =>
		new PlayerLaunchProbeMessage().Send();
}