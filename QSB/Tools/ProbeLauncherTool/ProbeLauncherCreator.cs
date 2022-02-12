using QSB.Player;
using QSB.PlayerBodySetup.Remote;

namespace QSB.Tools.ProbeLauncherTool
{
	internal static class ProbeLauncherCreator
	{
		internal static void CreateProbeLauncher(PlayerInfo player)
		{
			var REMOTE_ProbeLauncher = player.CameraBody.transform.Find("REMOTE_ProbeLauncher").gameObject;
			REMOTE_ProbeLauncher.SetActive(false);

			var REMOTE_Props_HEA_ProbeLauncher = REMOTE_ProbeLauncher.transform.Find("Props_HEA_ProbeLauncher");

			var tool = REMOTE_ProbeLauncher.GetComponent<QSBProbeLauncherTool>();
			tool.Type = ToolType.ProbeLauncher;
			tool.ToolGameObject = REMOTE_Props_HEA_ProbeLauncher.gameObject;
			tool.Player = player;

			FixMaterialsInAllChildren.ReplaceMaterials(REMOTE_ProbeLauncher.transform);

			//UnityEvents.FireInNUpdates(() => REMOTE_ProbeLauncher.SetActive(true), 5);
			REMOTE_ProbeLauncher.SetActive(true);
		}
	}
}
