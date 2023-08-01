using QSB.Player;

namespace QSB.Tools.SignalscopeTool;

public static class SignalscopeCreator
{
	internal static void CreateSignalscope(PlayerInfo player)
	{
		var REMOTE_Signalscope = player.CameraBody.transform.Find("REMOTE_Signalscope").gameObject;

		var Props_HEA_Signalscope = REMOTE_Signalscope.transform.Find("Props_HEA_Signalscope");

		var tool = REMOTE_Signalscope.GetComponent<QSBTool>();
		tool.Type = ToolType.Signalscope;
		tool.ToolGameObject = Props_HEA_Signalscope.gameObject;
		tool.Player = player;
	}
}