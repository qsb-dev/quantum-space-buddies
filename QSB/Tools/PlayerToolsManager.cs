using QSB.Player;
using QSB.Tools.FlashlightTool;
using QSB.Tools.ProbeLauncherTool;
using QSB.Tools.SignalscopeTool;
using QSB.Tools.TranslatorTool;
using QSB.Utility;
using System;
using System.Linq;
using UnityEngine;

namespace QSB.Tools
{
	public class PlayerToolsManager
	{
		//public static Material Props_HEA_PlayerTool_mat;
		//public static Material Props_HEA_Lightbulb_mat;
		//public static Material Props_HEA_Lightbulb_OFF_mat;
		//public static Material Structure_HEA_PlayerShip_Screens_mat;

		public static void InitRemote(PlayerInfo player)
		{
			try
			{
				FlashlightCreator.CreateFlashlight(player);
				SignalscopeCreator.CreateSignalscope(player);
				ProbeLauncherCreator.CreateProbeLauncher(player);
				TranslatorCreator.CreateTranslator(player);
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error when trying to create tools : {ex}", OWML.Common.MessageType.Error);
			}
		}

		public static void InitLocal()
		{
			var flashlight = Locator.GetFlashlight();
			var spot = flashlight._illuminationCheckLight;
			var lightLOD = spot.GetComponent<LightLOD>();

			if (lightLOD != null)
			{
				UnityEngine.Object.Destroy(lightLOD);
				spot.GetLight().shadows = LightShadows.Soft;
			}
		}
	}
}