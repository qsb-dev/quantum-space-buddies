using QSB.Player;
using QSB.Tools.FlashlightTool;
using QSB.Tools.ProbeLauncherTool;
using QSB.Tools.SignalscopeTool;
using QSB.Tools.TranslatorTool;
using QSB.Utility;
using System;
using UnityEngine;

namespace QSB.Tools
{
	public class PlayerToolsManager
	{
		public static void InitRemote(PlayerInfo player)
		{
			player.Try("create flashlight", () => FlashlightCreator.CreateFlashlight(player));
			player.Try("create signalscope", () => SignalscopeCreator.CreateSignalscope(player));
			player.Try("create probe launcher", () => ProbeLauncherCreator.CreateProbeLauncher(player));
			player.Try("create translator", () => TranslatorCreator.CreateTranslator(player));
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