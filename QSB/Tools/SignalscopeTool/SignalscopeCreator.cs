using QSB.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace QSB.Tools.SignalscopeTool
{
	internal static class SignalscopeCreator
	{
		private static readonly Vector3 SignalscopeScale = new(1.5f, 1.5f, 1.5f);

		internal static void CreateSignalscope(PlayerInfo player)
		{
			var signalscopeRoot = player.CameraBody.transform.Find("REMOTE_Signalscope").gameObject;

			signalscopeRoot.SetActive(false);

			var Props_HEA_Signalscope = signalscopeRoot.transform.Find("Props_HEA_Signalscope");

			var tool = signalscopeRoot.GetComponent<QSBTool>();
			tool.Type = ToolType.Signalscope;
			tool.ToolGameObject = Props_HEA_Signalscope.gameObject;
			tool.Player = player;

			Props_HEA_Signalscope.GetComponent<MeshRenderer>().material = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			Props_HEA_Signalscope.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;

			signalscopeRoot.SetActive(true);
		}
	}
}
