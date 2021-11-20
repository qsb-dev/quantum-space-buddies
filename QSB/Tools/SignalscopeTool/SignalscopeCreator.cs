using UnityEngine;

namespace QSB.Tools.SignalscopeTool
{
	internal static class SignalscopeCreator
	{
		private static readonly Vector3 SignalscopeScale = new(1.5f, 1.5f, 1.5f);

		internal static void CreateSignalscope(Transform cameraBody)
		{
			var signalscopeRoot = Object.Instantiate(GameObject.Find("Signalscope"));
			signalscopeRoot.name = "REMOTE_Signalscope";
			signalscopeRoot.SetActive(false);

			Object.Destroy(signalscopeRoot.GetComponent<SignalscopePromptController>());
			Object.Destroy(signalscopeRoot.transform.Find("Props_HEA_Signalscope")
				.Find("Props_HEA_Signalscope_Prepass").gameObject);

			var oldSignalscope = signalscopeRoot.GetComponent<Signalscope>();
			var tool = signalscopeRoot.AddComponent<QSBTool>();
			tool.MoveSpring = oldSignalscope._moveSpring;
			tool.StowTransform = PlayerToolsManager.StowTransform;
			tool.HoldTransform = PlayerToolsManager.HoldTransform;
			tool.ArrivalDegrees = 5f;
			tool.Type = ToolType.Signalscope;
			tool.ToolGameObject = signalscopeRoot.transform.Find("Props_HEA_Signalscope").gameObject;
			oldSignalscope.enabled = false;

			PlayerToolsManager.GetRenderer(signalscopeRoot, "Props_HEA_Signalscope").material = PlayerToolsManager.Props_HEA_PlayerTool_mat;

			signalscopeRoot.transform.parent = cameraBody;
			signalscopeRoot.transform.localPosition = Vector3.zero;
			signalscopeRoot.transform.localScale = SignalscopeScale;
			signalscopeRoot.SetActive(true);
		}
	}
}
