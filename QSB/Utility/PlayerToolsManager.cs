using OWML.ModHelper;
using OWML.ModHelper.Events;
using QSB.Animation;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Utility
{
    class PlayerToolsManager
    {
        private static Transform _rootBody;
        private static Transform _cameraRoot;

        public static void Init(Transform body, bool isLocal)
        {
            _rootBody = body;

            //_cameraRoot = _rootBody.Find("PlayerCamera");
            _cameraRoot = _rootBody;

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
            flashlightRoot.transform.parent = _cameraRoot;
            flashlightRoot.SetActive(true);
        }
    }
}
