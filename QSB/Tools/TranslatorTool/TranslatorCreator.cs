using QSB.Utility;
using UnityEngine;

namespace QSB.Tools.TranslatorTool
{
	internal static class TranslatorCreator
	{
		private static readonly Vector3 TranslatorScale = new Vector3(0.75f, 0.75f, 0.75f);

		internal static void CreateTranslator(Transform cameraBody)
		{
			var NomaiTranslatorProp = GameObject.Find("NomaiTranslatorProp");

			var REMOTE_NomaiTranslatorProp = NomaiTranslatorProp.InstantiateInactive();
			REMOTE_NomaiTranslatorProp.name = "REMOTE_NomaiTranslatorProp";

			var REMOTE_TranslatorGroup = REMOTE_NomaiTranslatorProp.transform.Find("TranslatorGroup");
			var REMOTE_Props_HEA_Translator = REMOTE_TranslatorGroup.Find("Props_HEA_Translator");

			var oldProp = REMOTE_NomaiTranslatorProp.GetComponent<NomaiTranslatorProp>();
			var newProp = REMOTE_NomaiTranslatorProp.AddComponent<QSBNomaiTranslatorProp>();
			newProp.TranslatorProp = REMOTE_TranslatorGroup.gameObject;

			Object.Destroy(REMOTE_NomaiTranslatorProp.GetComponent<NomaiTranslatorProp>());
			Object.Destroy(REMOTE_TranslatorGroup.Find("Canvas").gameObject);
			//Object.Destroy(REMOTE_TranslatorGroup.Find("Lighting").gameObject);
			//Object.Destroy(REMOTE_TranslatorGroup.Find("TranslatorBeams").gameObject);
			Object.Destroy(REMOTE_Props_HEA_Translator.Find("Props_HEA_Translator_Pivot_RotatingPart")
				.Find("Props_HEA_Translator_RotatingPart")
				.Find("Props_HEA_Translator_RotatingPart_Prepass").gameObject);
			Object.Destroy(REMOTE_Props_HEA_Translator.Find("Props_HEA_Translator_Prepass").gameObject);

			var oldTranslator = REMOTE_NomaiTranslatorProp.GetComponent<NomaiTranslator>();
			var tool = REMOTE_NomaiTranslatorProp.AddComponent<QSBNomaiTranslator>();
			tool.MoveSpring = oldTranslator._moveSpring;
			tool.StowTransform = PlayerToolsManager.StowTransform;
			tool.HoldTransform = PlayerToolsManager.HoldTransform;
			tool.ArrivalDegrees = 5f;
			tool.Type = ToolType.Translator;
			tool.ToolGameObject = REMOTE_TranslatorGroup.gameObject;
			tool.RaycastTransform = cameraBody;
			Object.Destroy(oldTranslator);

			PlayerToolsManager.GetRenderer(REMOTE_NomaiTranslatorProp, "Props_HEA_Translator_Screen").material = PlayerToolsManager.Structure_HEA_PlayerShip_Screens_mat;
			PlayerToolsManager.GetRenderer(REMOTE_NomaiTranslatorProp, "Props_HEA_Translator_Geo").material = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			PlayerToolsManager.GetRenderer(REMOTE_NomaiTranslatorProp, "Props_HEA_Translator_RotatingPart").material = PlayerToolsManager.Props_HEA_PlayerTool_mat;
			PlayerToolsManager.GetRenderer(REMOTE_NomaiTranslatorProp, "Props_HEA_Translator_Button_L").material = PlayerToolsManager.Props_HEA_Lightbulb_mat;
			PlayerToolsManager.GetRenderer(REMOTE_NomaiTranslatorProp, "Props_HEA_Translator_Button_R").material = PlayerToolsManager.Props_HEA_Lightbulb_mat;

			REMOTE_NomaiTranslatorProp.transform.parent = cameraBody;
			REMOTE_NomaiTranslatorProp.transform.localPosition = Vector3.zero;
			REMOTE_NomaiTranslatorProp.transform.localScale = TranslatorScale;
			QSBCore.UnityEvents.FireOnNextUpdate(() => REMOTE_NomaiTranslatorProp.SetActive(true));
		}
	}
}
