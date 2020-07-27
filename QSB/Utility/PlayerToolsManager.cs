using OWML.ModHelper.Events;
using QSB.Animation;
using UnityEngine;

namespace QSB.Utility
{
    class PlayerToolsManager
    {
        private static Transform _cameraBody;
        private static readonly Vector3 FlashlightOffset = new Vector3(0.7196316f, -0.2697681f, 0.3769455f);

        public static void Init(Transform camera)
        {
            _cameraBody = camera;

            CreateFlashlight();
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
    }
}
