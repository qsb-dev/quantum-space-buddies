using QSB.Player;
using QSB.Tools.FlashlightTool;
using QSB.Tools.ProbeLauncherTool;
using QSB.Tools.SignalscopeTool;
using QSB.Tools.TranslatorTool;
using QSB.Utility;
using UnityEngine;

namespace QSB.Tools;

public class PlayerToolsManager
{
	public static void InitRemote(PlayerInfo player)
	{
		player.PlayerId.Try("creating flashlight", () => FlashlightCreator.CreateFlashlight(player));
		player.PlayerId.Try("creating signalscope", () => SignalscopeCreator.CreateSignalscope(player));
		player.PlayerId.Try("creating probe launcher", () => ProbeLauncherCreator.CreateProbeLauncher(player));
		player.PlayerId.Try("creating translator", () => TranslatorCreator.CreateTranslator(player));
	}

	public static void InitLocal()
	{
		var flashlight = Locator.GetFlashlight();
		var spot = flashlight._illuminationCheckLight;
		var lightLOD = spot.GetComponent<LightLOD>();

		if (lightLOD != null)
		{
			Object.Destroy(lightLOD);
			spot.GetLight().shadows = LightShadows.Soft;
		}
	}
}