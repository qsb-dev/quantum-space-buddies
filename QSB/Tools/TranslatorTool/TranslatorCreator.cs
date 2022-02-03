using QSB.Player;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Rendering;

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
		}
	}
}
