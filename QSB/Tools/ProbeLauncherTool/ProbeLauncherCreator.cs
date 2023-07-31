using QSB.Player;

namespace QSB.Tools.ProbeLauncherTool;

public static class ProbeLauncherCreator
{
	internal static void CreateProbeLauncher(PlayerInfo player)
	{
		var REMOTE_ProbeLauncher = player.CameraBody.transform.Find("REMOTE_ProbeLauncher").gameObject;

		var REMOTE_Props_HEA_ProbeLauncher = REMOTE_ProbeLauncher.transform.Find("Props_HEA_ProbeLauncher");

		var tool = REMOTE_ProbeLauncher.GetComponent<QSBProbeLauncherTool>();
		tool.Type = ToolType.ProbeLauncher;
		tool.ToolGameObject = REMOTE_Props_HEA_ProbeLauncher.gameObject;
		tool.Player = player;
	}
}