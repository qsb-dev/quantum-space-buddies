using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.TranslatorTool
{
	internal static class TranslatorCreator
	{
		private static readonly Vector3 TranslatorScale = new Vector3(0.75f, 0.75f, 0.75f);

		internal static void CreateTranslator(Transform cameraBody)
		{
			var original = GameObject.Find("NomaiTranslatorProp");

			var translatorRoot = original.InstantiateInactive();
			translatorRoot.name = "REMOTE_NomaiTranslatorProp";

			var group = translatorRoot.transform.Find("TranslatorGroup");
			var model = group.Find("Props_HEA_Translator");

			Object.Destroy(translatorRoot.GetComponent<NomaiTranslatorProp>());
			Object.Destroy(group.Find("Canvas").gameObject);
			Object.Destroy(group.Find("Lighting").gameObject);
			Object.Destroy(group.Find("TranslatorBeams").gameObject);
			Object.Destroy(model.Find("Props_HEA_Translator_Pivot_RotatingPart")
				.Find("Props_HEA_Translator_RotatingPart")
				.Find("Props_HEA_Translator_RotatingPart_Prepass").gameObject);
			Object.Destroy(model.Find("Props_HEA_Translator_Prepass").gameObject);

			var oldTranslator = translatorRoot.GetComponent<NomaiTranslator>();
			var tool = translatorRoot.AddComponent<QSBTool>();
			tool.MoveSpring = oldTranslator._moveSpring;
			tool.StowTransform = PlayerToolsManager.StowTransform;
			tool.HoldTransform = PlayerToolsManager.HoldTransform;
			tool.ArrivalDegrees = 5f;
			tool.Type = ToolType.Translator;
			tool.ToolGameObject = group.gameObject;
			Object.Destroy(oldTranslator);

			PlayerToolsManager.GetRenderer(translatorRoot, "Props_HEA_Translator_Geo").material = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			PlayerToolsManager.GetRenderer(translatorRoot, "Props_HEA_Translator_RotatingPart").material = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			PlayerToolsManager.GetRenderer(translatorRoot, "Props_HEA_Translator_Button_L").material = PlayerToolsManager.Props_HEA_Lightbulb_mat;
			PlayerToolsManager.GetRenderer(translatorRoot, "Props_HEA_Translator_Button_R").material = PlayerToolsManager.Props_HEA_Lightbulb_mat;

			translatorRoot.transform.parent = cameraBody;
			translatorRoot.transform.localPosition = Vector3.zero;
			translatorRoot.transform.localScale = TranslatorScale;
			QSBCore.UnityEvents.FireOnNextUpdate(() => translatorRoot.SetActive(true));
		}
	}
}
