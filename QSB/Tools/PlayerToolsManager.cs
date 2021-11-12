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

		public static void Init(Transform playerCamera)
		{
			try
			{
				CreateStowTransforms(playerCamera);

				Props_HEA_PlayerTool_mat = GameObject.Find("Props_HEA_ProbeLauncher_ProbeCamera/ProbeLauncherChassis").GetComponent<MeshRenderer>().materials[0];
				Props_HEA_Lightbulb_OFF_mat = GameObject.Find("Props_HEA_Probe_Prelaunch").GetComponent<MeshRenderer>().materials[1];

				if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
				{
					Structure_HEA_PlayerShip_Screens_mat = GameObject.Find("ProbeScreen (1)/ProbeScreenPivot/ProbeScreen").GetComponent<MeshRenderer>().materials[2];
					Props_HEA_Lightbulb_mat = GameObject.Find("Props_HEA_Lantern (10)/Lantern_Lamp").GetComponent<MeshRenderer>().materials[0];
					Props_HEA_Lightbulb_OFF_mat = GameObject.Find("NomaiResearchExhibit/Props_HEA_Probe_STATIC").GetComponent<MeshRenderer>().materials[1];
				}
				else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
				{
					Props_HEA_Lightbulb_mat = GameObject.Find("lantern_lamp").GetComponent<MeshRenderer>().materials[0];

					// BUG : uhhhhh fuckin' uhhhhhhhh (find a material)
					Props_HEA_Lightbulb_OFF_mat = null;
					Structure_HEA_PlayerShip_Screens_mat = null;
				}

			}
			catch (Exception ex)
			{
				DebugLog.ToConsole($"Error when trying to find materials : {ex}", OWML.Common.MessageType.Error);
			}

			FlashlightCreator.CreateFlashlight(playerCamera);
			SignalscopeCreator.CreateSignalscope(playerCamera);
			ProbeLauncherCreator.CreateProbeLauncher(playerCamera);
			TranslatorCreator.CreateTranslator(playerCamera);
		}

		private static void CreateStowTransforms(Transform cameraBody)
		{
			var stow = new GameObject("REMOTE_ToolStowTransform");
			StowTransform = stow.transform;
			stow.transform.parent = cameraBody;
			stow.transform.localPosition = Vector3.zero;
			stow.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);

			var hold = new GameObject("REMOTE_ToolHoldTransform");
			HoldTransform = hold.transform;
			hold.transform.parent = cameraBody;
			hold.transform.localPosition = Vector3.zero;
			hold.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

			var item = new GameObject("REMOTE_ItemSocket");
			item.transform.parent = cameraBody;
			item.transform.localPosition = new Vector3(0.125f, -0.12f, 0.2f);
			item.transform.localRotation = Quaternion.Euler(0, 0, 15);
			item.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);

			var scroll = new GameObject("REMOTE_ScrollSocket");
			scroll.transform.parent = cameraBody;
			scroll.transform.localPosition = new Vector3(0.148f, -0.0522f, 0.2465f);
			scroll.transform.localRotation = Quaternion.Euler(236.054f, 56.46799f, -152.472f);
			scroll.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);

			var sharedStone = new GameObject("REMOTE_SharedStoneSocket");
			sharedStone.transform.parent = cameraBody;
			sharedStone.transform.localPosition = new Vector3(0.1375f, -0.119f, 0.2236f);
			sharedStone.transform.localRotation = Quaternion.Euler(-23.053f, -0.263f, 6.704f);
			sharedStone.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);

			var warpCore = new GameObject("REMOTE_WarpCoreSocket");
			warpCore.transform.parent = cameraBody;
			warpCore.transform.localPosition = new Vector3(0.161f, -0.107f, 0.223f);
			warpCore.transform.localRotation = Quaternion.Euler(179.949f, 82.59f, 163.697f);
			warpCore.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);

			var vesselCore = new GameObject("REMOTE_VesselCoreSocket");
			vesselCore.transform.parent = cameraBody;
			vesselCore.transform.localPosition = new Vector3(0.177f, -0.106f, 0.2f);
			vesselCore.transform.localRotation = Quaternion.Euler(3.142f, 14.827f, 12.094f);
			vesselCore.transform.localScale = new Vector3(0.27f, 0.27f, 0.27f);

			var simpleLantern = new GameObject("REMOTE_SimpleLanternSocket");
			simpleLantern.transform.parent = cameraBody;
			simpleLantern.transform.localPosition = new Vector3(0.242997f, -0.18f, 0.2620007f);
			simpleLantern.transform.localRotation = Quaternion.Euler(0f, 33f, 0f);
			simpleLantern.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);

			var dreamLantern = new GameObject("REMOTE_DreamLanternSocket");
			dreamLantern.transform.parent = cameraBody;
			dreamLantern.transform.localPosition = new Vector3(0.243f, -0.207f, 0.262f);
			dreamLantern.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			dreamLantern.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);

			var slideReel = new GameObject("REMOTE_SlideReelSocket");
			slideReel.transform.parent = cameraBody;
			slideReel.transform.localPosition = new Vector3(0.1353f, -0.0878f, 0.2878f);
			slideReel.transform.localRotation = Quaternion.Euler(-145.532f, 6.589996f, -94.54901f);
			slideReel.transform.localScale = new Vector3(0.3300001f, 0.33f, 0.3299999f);

			var visionTorch = new GameObject("REMOTE_VisionTorchSocket");
			visionTorch.transform.parent = cameraBody;
			visionTorch.transform.localPosition = new Vector3(0.21f, -0.32f, 0.33f);
			visionTorch.transform.localRotation = Quaternion.Euler(-4.5f, 0.03f, 9f);
			visionTorch.transform.localScale = new Vector3(0.33f, 0.33f, 0.33f);
		}

		internal static MeshRenderer GetRenderer(GameObject root, string gameObjectName) =>
			root.GetComponentsInChildren<MeshRenderer>(true).First(x => x.name == gameObjectName);
	}
}