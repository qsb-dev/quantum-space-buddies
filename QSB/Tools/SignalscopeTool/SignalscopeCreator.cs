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
			var signalscopeRoot = Object.Instantiate(GameObject.Find("Signalscope"));
			signalscopeRoot.name = "REMOTE_Signalscope";
			signalscopeRoot.SetActive(false);

			var Props_HEA_Signalscope = signalscopeRoot.transform.Find("Props_HEA_Signalscope");

			Object.Destroy(signalscopeRoot.GetComponent<SignalscopePromptController>());
			Object.Destroy(Props_HEA_Signalscope.Find("Props_HEA_Signalscope_Prepass").gameObject);

			var oldSignalscope = signalscopeRoot.GetComponent<Signalscope>();
			var tool = signalscopeRoot.AddComponent<QSBTool>();
			tool.MoveSpring = oldSignalscope._moveSpring;
			tool.StowTransform = PlayerToolsManager.StowTransform;
			tool.HoldTransform = PlayerToolsManager.HoldTransform;
			tool.ArrivalDegrees = 5f;
			tool.Type = ToolType.Signalscope;
			tool.ToolGameObject = Props_HEA_Signalscope.gameObject;
			tool.Player = player;
			oldSignalscope.enabled = false;

			Props_HEA_Signalscope.GetComponent<MeshRenderer>().material = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			Props_HEA_Signalscope.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.On;

			signalscopeRoot.transform.parent = player.CameraBody.transform;
			signalscopeRoot.transform.localPosition = Vector3.zero;
			signalscopeRoot.transform.localScale = SignalscopeScale;
			signalscopeRoot.SetActive(true);
		}
	}
}
