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
		public static Transform StowTransform;
		public static Transform HoldTransform;

		public static Material Props_HEA_PlayerTool_mat;
		public static Material Props_HEA_Lightbulb_mat;
		public static Material Props_HEA_Lightbulb_OFF_mat;
		public static Material Structure_HEA_PlayerShip_Screens_mat;

		public static void InitRemote(PlayerInfo player)
		{
			try
			{
				CreateStowTransforms(player.CameraBody.transform);

				var surfaceData = Locator.GetSurfaceManager()._surfaceLookupAsset;
				var metal = surfaceData.surfaceTypeGroups[15].materials;
				var glass = surfaceData.surfaceTypeGroups[19].materials;

				Props_HEA_PlayerTool_mat = metal[27];
				Props_HEA_Lightbulb_mat = glass[47];
				Props_HEA_Lightbulb_OFF_mat = glass[48];
				Structure_HEA_PlayerShip_Screens_mat = glass[41];
			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error when trying to find materials : {ex}", OWML.Common.MessageType.Error);
			}

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

		private static void CreateStowTransforms(Transform cameraBody)
		{
			StowTransform = cameraBody.Find("REMOTE_ToolStowTransform");
			HoldTransform = cameraBody.Find("REMOTE_ToolHoldTransform");
		}

		internal static MeshRenderer GetRenderer(GameObject root, string gameObjectName) =>
			root.GetComponentsInChildren<MeshRenderer>(true).First(x => x.name == gameObjectName);
	}
}