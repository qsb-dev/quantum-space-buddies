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

        private static readonly Vector3 FlashlightOffset = new Vector3(0.7196316f, -0.2697681f, 0.3769455f);
        private static readonly Vector3 SignalscopeScale = new Vector3(1.5f, 1.5f, 1.5f);


        public static void Init(Transform camera)
        {
            _cameraBody = camera;
            CreateStowTransform(_cameraBody);

            CreateFlashlight();
            CreateSignalscope();
        }

        private static void CreateStowTransform(Transform root)
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
            tool._scopeGameObject = signalscopeRoot.transform.Find("Props_HEA_Signalscope").gameObject;
            signalscopeRoot.transform.Find("Props_HEA_Signalscope").gameObject.layer = 0;
            oldSignalscope.enabled = false;

            signalscopeRoot.transform.parent = _cameraBody;
            signalscopeRoot.transform.localPosition = Vector3.zero;
            signalscopeRoot.transform.localScale = SignalscopeScale;
            signalscopeRoot.SetActive(true);
        }
    }
}
