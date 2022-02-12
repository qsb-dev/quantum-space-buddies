using QSB.Player;
using QSB.PlayerBodySetup.Remote;

namespace QSB.Tools.TranslatorTool
{
	internal static class TranslatorCreator
	{
		internal static void CreateTranslator(PlayerInfo player)
		{
			var REMOTE_NomaiTranslatorProp = player.CameraBody.transform.Find("REMOTE_NomaiTranslatorProp").gameObject;

			var REMOTE_TranslatorGroup = REMOTE_NomaiTranslatorProp.transform.Find("TranslatorGroup");

			var tool = REMOTE_NomaiTranslatorProp.GetComponent<QSBNomaiTranslator>();
			tool.Type = ToolType.Translator;
			tool.ToolGameObject = REMOTE_TranslatorGroup.gameObject;
			tool.Player = player;

			FixMaterialsInAllChildren.ReplaceMaterials(REMOTE_NomaiTranslatorProp.transform);

			REMOTE_NomaiTranslatorProp.SetActive(true);
		}
	}
}
