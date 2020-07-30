using OWML.ModHelper.Events;
using QSB.Animation;
using UnityEngine;

namespace QSB.Utility
{
    public class PlayerToolsManager
    {
        private static Transform _cameraBody;
        private static Transform _toolStowTransform;
        private static Transform _toolHoldTransform;
        private static Material _playerToolsMaterial;
        private static Material _lightbulbMaterial;

        private static readonly Vector3 FlashlightOffset = new Vector3(0.7196316f, -0.2697681f, 0.3769455f);
        private static readonly Vector3 SignalscopeScale = new Vector3(1.5f, 1.5f, 1.5f);
        private static readonly Vector3 TranslatorScale = new Vector3(1, 1, 1);

        public static void Init(Transform camera)
        {
            _cameraBody = camera;
            CreateStowTransforms(_cameraBody);

            CreateFlashlight();
            CreateSignalscope();
            CreateTranslator();

            _playerToolsMaterial = GameObject.Find("PlayerSuit_Jetpack").GetComponent<MeshRenderer>().materials[0];
            _lightbulbMaterial = GameObject.Find("Props_HEA_Lantern (10)/Lantern_Lamp").GetComponent<MeshRenderer>().material;
        }

        private static void CreateStowTransforms(Transform root)
        {
            var stow = new GameObject("ToolStowTransform");
            _toolStowTransform = stow.transform;
            stow.transform.parent = root;
            stow.transform.localPosition = Vector3.zero;
            stow.transform.localRotation = Quaternion.Euler(45f, 0f, 0f);

            var hold = new GameObject("ToolHoldTransform");
            _toolHoldTransform = hold.transform;
            hold.transform.parent = root;
            hold.transform.localPosition = Vector3.zero;
            hold.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        private static void CreateFlashlight()
        {
            var flashlightRoot = GameObject.Instantiate(GameObject.Find("FlashlightRoot"));
            flashlightRoot.SetActive(false);
            var oldComponent = flashlightRoot.GetComponent<Flashlight>();
            var component = flashlightRoot.AddComponent<QSBFlashlight>();
            component._lights = oldComponent.GetValue<OWLight2[]>("_lights");
            component._illuminationCheckLight = oldComponent.GetValue<OWLight2>("_illuminationCheckLight");
            component._root = oldComponent.GetValue<Transform>("_root");
            component._basePivot = oldComponent.GetValue<Transform>("_basePivot");
            component._wobblePivot = oldComponent.GetValue<Transform>("_wobblePivot");
            oldComponent.enabled = false;
            flashlightRoot.transform.parent = _cameraBody;
            flashlightRoot.transform.localPosition = FlashlightOffset;
            flashlightRoot.SetActive(true);
        }

        private static void CreateSignalscope()
        {
            var signalscopeRoot = GameObject.Instantiate(GameObject.Find("Signalscope"));
            signalscopeRoot.SetActive(false);
            Object.Destroy(signalscopeRoot.GetComponent<SignalscopePromptController>());
            Object.Destroy(signalscopeRoot.transform.Find("Props_HEA_Signalscope_Prepass"));

            var oldSignalscope = signalscopeRoot.GetComponent<Signalscope>();
            var tool = signalscopeRoot.AddComponent<QSBTool>();
            tool.SetValue("_moveSpring", oldSignalscope.GetValue<DampedSpringQuat>("_moveSpring"));
            tool.SetValue("_stowTransform", _toolStowTransform);
            tool.SetValue("_holdTransform", _toolHoldTransform);
            tool.SetValue("_arrivalDegrees", 5f);
            tool.Type = ToolType.Signalscope;
            tool.ToolGameObject = signalscopeRoot.transform.Find("Props_HEA_Signalscope").gameObject;
            oldSignalscope.enabled = false;

            signalscopeRoot.transform.Find("Props_HEA_Signalscope").GetComponent<MeshRenderer>().material = _playerToolsMaterial;

            signalscopeRoot.transform.parent = _cameraBody;
            signalscopeRoot.transform.localPosition = Vector3.zero;
            signalscopeRoot.transform.localScale = SignalscopeScale;
            signalscopeRoot.SetActive(true);
        }

        private static void CreateTranslator()
        {
            var translatorRoot = GameObject.Instantiate(GameObject.Find("NomaiTranslatorProp"));
            translatorRoot.SetActive(false);

            Object.Destroy(translatorRoot.GetComponent<NomaiTranslatorProp>());
            Object.Destroy(translatorRoot.transform.Find("Canvas"));
            Object.Destroy(translatorRoot.transform.Find("TranslatorBeams"));
            Object.Destroy(translatorRoot.transform.Find("Lighting"));
            Object.Destroy(translatorRoot.transform.Find("Props_HEA_Translator_RotatingPart_Prepass"));
            Object.Destroy(translatorRoot.transform.Find("Props_HEA_Translator_Prepass"));

            var oldTranslator = translatorRoot.GetComponent<NomaiTranslator>();
            var tool = translatorRoot.AddComponent<QSBTool>();
            tool.SetValue("_moveSpring", oldTranslator.GetValue<DampedSpringQuat>("_moveSpring"));
            tool.SetValue("_stowTransform", _toolStowTransform);
            tool.SetValue("_holdTransform", _toolHoldTransform);
            tool.SetValue("_arrivalDegrees", 5f);
            tool.Type = ToolType.Translator;
            tool.ToolGameObject = translatorRoot.transform.Find("TranslatorGroup").gameObject;
            oldTranslator.enabled = false;

            translatorRoot.transform.Find("Props_HEA_Translator_Geo").GetComponent<MeshRenderer>().material = _playerToolsMaterial;
            translatorRoot.transform.Find("Props_HEA_Translator_RotatingPart").GetComponent<MeshRenderer>().material = _playerToolsMaterial;
            translatorRoot.transform.Find("Props_HEA_Translator_Button_L").GetComponent<MeshRenderer>().material = _lightbulbMaterial;
            translatorRoot.transform.Find("Props_HEA_Translator_Button_R").GetComponent<MeshRenderer>().material = _lightbulbMaterial;

            translatorRoot.transform.parent = _cameraBody;
            translatorRoot.transform.localPosition = Vector3.zero;
            translatorRoot.transform.localScale = TranslatorScale;
            translatorRoot.SetActive(true);
        }
    }
}
