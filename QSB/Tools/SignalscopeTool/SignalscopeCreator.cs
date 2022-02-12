using QSB.Player;
using QSB.PlayerBodySetup.Remote;

namespace QSB.Tools.SignalscopeTool
{
	internal static class SignalscopeCreator
	{
		internal static void CreateSignalscope(PlayerInfo player)
		{
			var signalscopeRoot = player.CameraBody.transform.Find("REMOTE_Signalscope").gameObject;

			signalscopeRoot.SetActive(false);

			var Props_HEA_Signalscope = signalscopeRoot.transform.Find("Props_HEA_Signalscope");

			var tool = signalscopeRoot.GetComponent<QSBTool>();
			tool.Type = ToolType.Signalscope;
			tool.ToolGameObject = Props_HEA_Signalscope.gameObject;
			tool.Player = player;

			FixMaterialsInAllChildren.ReplaceMaterials(signalscopeRoot.transform);

			signalscopeRoot.SetActive(true);
		}
	}
}
