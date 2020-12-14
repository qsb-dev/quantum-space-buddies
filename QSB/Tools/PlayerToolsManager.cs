using OWML.ModHelper.Events;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.Tools
{
	public class PlayerToolsManager
	{
		private static Transform _toolStowTransform;
		private static Transform _toolHoldTransform;
		private static Material _playerToolsMaterial;
		private static Material _lightbulbMaterial;

		private static readonly Vector3 FlashlightOffset = new Vector3(0.7196316f, -0.2697681f, 0.3769455f);
		private static readonly Vector3 ProbeLauncherOffset = new Vector3(0.5745087f, -0.26f, 0.4453125f);
		private static readonly Vector3 SignalscopeScale = new Vector3(1.5f, 1.5f, 1.5f);
		private static readonly Vector3 TranslatorScale = new Vector3(0.75f, 0.75f, 0.75f);

		public static void Init(Transform camera)
		{
			CreateStowTransforms(camera);

			_playerToolsMaterial = GameObject.Find("Props_HEA_ProbeLauncher_ProbeCamera/ProbeLauncherChassis").GetComponent<MeshRenderer>().materials[0];
			if (QSBSceneManager.CurrentScene == OWScene.SolarSystem)
			{
				_lightbulbMaterial = GameObject.Find("Props_HEA_Lantern (10)/Lantern_Lamp").GetComponent<MeshRenderer>().materials[0];
			}
			else if (QSBSceneManager.CurrentScene == OWScene.EyeOfTheUniverse)
			{
				_lightbulbMaterial = GameObject.Find("lantern_lamp").GetComponent<MeshRenderer>().materials[0];
			}

			CreateFlashlight(camera);
			CreateSignalscope(camera);
			CreateProbeLauncher(camera);
			CreateTranslator(camera);
		}

		public static void CreateProbe(Transform body, PlayerInfo player)
		{
			var newProbe = body.gameObject.AddComponent<QSBProbe>();
			player.Probe = newProbe;
		}

		private static void CreateStowTransforms(Transform cameraBody)
		{
			var stow = new GameObject("ToolStowTransform");
			_toolStowTransform = stow.transform;
			stow.transform.parent = cameraBody;
			stow.transform.localPosition = Vector3.zero;
			stow.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);

			var hold = new GameObject("ToolHoldTransform");
			_toolHoldTransform = hold.transform;
			hold.transform.parent = cameraBody;
			hold.transform.localPosition = Vector3.zero;
			hold.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}

		private static void CreateFlashlight(Transform cameraBody)
		{
			var flashlightRoot = Object.Instantiate(GameObject.Find("FlashlightRoot"));
			flashlightRoot.SetActive(false);
			var oldComponent = flashlightRoot.GetComponent<Flashlight>();
			var component = flashlightRoot.AddComponent<QSBFlashlight>();

			component.Init(oldComponent);
			oldComponent.enabled = false;

			flashlightRoot.transform.parent = cameraBody;
			flashlightRoot.transform.localPosition = FlashlightOffset;
			flashlightRoot.SetActive(true);
        }

		private static void CreateSignalscope(Transform cameraBody)
		{
			var signalscopeRoot = Object.Instantiate(GameObject.Find("Signalscope"));
			signalscopeRoot.SetActive(false);

			Object.Destroy(signalscopeRoot.GetComponent<SignalscopePromptController>());
			Object.Destroy(signalscopeRoot.transform.Find("Props_HEA_Signalscope")
				.Find("Props_HEA_Signalscope_Prepass").gameObject);

			var oldSignalscope = signalscopeRoot.GetComponent<Signalscope>();
			var tool = signalscopeRoot.AddComponent<QSBTool>();
			tool.MoveSpring = oldSignalscope.GetValue<DampedSpringQuat>("_moveSpring");
			tool.StowTransform = _toolStowTransform;
			tool.HoldTransform = _toolHoldTransform;
			tool.ArrivalDegrees = 5f;
			tool.Type = ToolType.Signalscope;
			tool.ToolGameObject = signalscopeRoot.transform.Find("Props_HEA_Signalscope").gameObject;
			oldSignalscope.enabled = false;

			GetRenderer(signalscopeRoot, "Props_HEA_Signalscope").material = _playerToolsMaterial;

			signalscopeRoot.transform.parent = cameraBody;
			signalscopeRoot.transform.localPosition = Vector3.zero;
			signalscopeRoot.transform.localScale = SignalscopeScale;
			signalscopeRoot.SetActive(true);
        }

		private static void CreateTranslator(Transform cameraBody)
		{
			var original = GameObject.Find("NomaiTranslatorProp");

			var translatorRoot = original.InstantiateInactive();

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
			tool.MoveSpring = oldTranslator.GetValue<DampedSpringQuat>("_moveSpring");
			tool.StowTransform = _toolStowTransform;
			tool.HoldTransform = _toolHoldTransform;
			tool.ArrivalDegrees = 5f;
			tool.Type = ToolType.Translator;
			tool.ToolGameObject = group.gameObject;
			Object.Destroy(oldTranslator);

			GetRenderer(translatorRoot, "Props_HEA_Translator_Geo").material = _playerToolsMaterial;
			GetRenderer(translatorRoot, "Props_HEA_Translator_RotatingPart").material = _playerToolsMaterial;
			GetRenderer(translatorRoot, "Props_HEA_Translator_Button_L").material = _lightbulbMaterial;
			GetRenderer(translatorRoot, "Props_HEA_Translator_Button_R").material = _lightbulbMaterial;

			translatorRoot.transform.parent = cameraBody;
			translatorRoot.transform.localPosition = Vector3.zero;
			translatorRoot.transform.localScale = TranslatorScale;
			QSBCore.Helper.Events.Unity.FireOnNextUpdate(() => translatorRoot.SetActive(true));
        }

		private static void CreateProbeLauncher(Transform cameraBody)
		{
			var launcherRoot = new GameObject("ProbeLauncher");
			var modelOrig = GameObject.Find("PlayerCamera/ProbeLauncher/Props_HEA_ProbeLauncher");
			var model = Object.Instantiate(modelOrig);
			model.transform.parent = launcherRoot.transform;

			Object.Destroy(model.transform.Find("Props_HEA_ProbeLauncher_Prepass").gameObject);
			Object.Destroy(model.transform.Find("Props_HEA_Probe_Prelaunch").Find("Props_HEA_Probe_Prelaunch_Prepass").gameObject);

			var tool = launcherRoot.AddComponent<QSBTool>();
			var spring = new DampedSpringQuat
			{
				velocity = Vector4.zero,
				settings = new DampedSpringSettings
				{
					springConstant = 50f,
					dampingCoefficient = 8.485282f,
					mass = 1
				}
			};
			tool.MoveSpring = spring;
			tool.StowTransform = _toolStowTransform;
			tool.HoldTransform = _toolHoldTransform;
			tool.ArrivalDegrees = 5f;
			tool.Type = ToolType.ProbeLauncher;
			tool.ToolGameObject = model;

			GetRenderer(launcherRoot, "Props_HEA_Probe_Prelaunch").materials[0] = _playerToolsMaterial;
			GetRenderer(launcherRoot, "Props_HEA_Probe_Prelaunch").materials[1] = _lightbulbMaterial;
			GetRenderer(launcherRoot, "PressureGauge_Arrow").material = _playerToolsMaterial;
			GetRenderer(launcherRoot, "ProbeLauncherChassis").material = _playerToolsMaterial;

			launcherRoot.transform.parent = cameraBody;
			launcherRoot.transform.localPosition = ProbeLauncherOffset;
			launcherRoot.SetActive(true);
        }

		private static MeshRenderer GetRenderer(GameObject root, string gameObjectName) => 
            root.GetComponentsInChildren<MeshRenderer>(true).First(x => x.name == gameObjectName);
    }
}