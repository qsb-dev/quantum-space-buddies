using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace QSB.Instruments.QSBCamera
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance;
        private GameObject CameraBase;
        private GameObject CameraObj;
        private Camera Camera;
        private OWCamera OWCamera;
        public bool IsSetUp { get; private set; }
        public CameraMode Mode { get; private set; }

        public void Start()
        {
            Instance = this;
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(OWScene scene, bool inUniverse)
        {
            if (!inUniverse)
            {
                return;
            }
            IsSetUp = false;
            QSB.Helper.Events.Unity.RunWhen(() => Locator.GetPlayerCamera() != null && Locator.GetPlayerTransform() != null, FinishSetup);
        }

        private void FinishSetup()
        {
            CameraBase = new GameObject();
            CameraBase.SetActive(false);
            CameraBase.AddComponent<Transform>();
            CameraBase.transform.parent = Locator.GetPlayerTransform();
            CameraBase.transform.localPosition = Vector3.zero;
            CameraBase.transform.localRotation = Quaternion.Euler(0, 0, 0);

            CameraObj = new GameObject();
            CameraObj.transform.parent = CameraBase.transform;
            CameraObj.transform.localPosition = new Vector3(0, 0.8f, -5f);
            CameraObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Camera = CameraObj.AddComponent<Camera>();
            Camera.cullingMask = Locator.GetPlayerCamera().mainCamera.cullingMask & ~(1 << 27) | (1 << 22);
            Camera.clearFlags = CameraClearFlags.Color;
            Camera.backgroundColor = Color.black;
            Camera.fieldOfView = 90f;
            Camera.nearClipPlane = 0.1f;
            Camera.farClipPlane = 40000f;
            Camera.depth = 0f;
            Camera.enabled = false;
            OWCamera = CameraObj.AddComponent<OWCamera>();
            OWCamera.renderSkybox = true;
            CameraObj.AddComponent<CameraController>();

            var screenGrab = CameraObj.AddComponent<FlashbackScreenGrabImageEffect>();
            screenGrab._downsampleShader = Locator.GetPlayerCamera().gameObject.GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader;

            var fogImage = CameraObj.AddComponent<PlanetaryFogImageEffect>();
            fogImage.fogShader = Locator.GetPlayerCamera().gameObject.GetComponent<PlanetaryFogImageEffect>().fogShader;

            //var postProcessing = CameraObj.AddComponent<PostProcessingBehaviour>();
            //postProcessing.profile = Locator.GetPlayerCamera().gameObject.GetAddComponent<PostProcessingBehaviour>().profile;

            CameraBase.SetActive(true);

            IsSetUp = true;
        }

        public void SwitchTo3rdPerson()
        {
            if (!IsSetUp)
            {
                DebugLog.ToConsole("Warning - Camera not set up!", MessageType.Warning);
                return;
            }
            if (Mode == CameraMode.ThirdPerson)
            {
                DebugLog.ToConsole("Warning - Already in 3rd person!", MessageType.Warning);
                return;
            }
            if (OWInput.GetInputMode() != InputMode.Character)
            {
                DebugLog.ToConsole("Warning - Cannot change to 3rd person while not in Character inputmode!", MessageType.Warning);
                return;
            }
            OWInput.ChangeInputMode(InputMode.None);
            GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", OWCamera);
            Locator.GetActiveCamera().mainCamera.enabled = false;
            Camera.enabled = true;
            Mode = CameraMode.ThirdPerson;
        }

        public void SwitchTo1stPerson()
        {
            if (!IsSetUp)
            {
                DebugLog.ToConsole("Warning - Camera not set up!", MessageType.Warning);
                return;
            }
            if (Mode == CameraMode.FirstPerson)
            {
                DebugLog.ToConsole("Warning - Already in 1st person!", MessageType.Warning);
                return;
            }
            OWInput.ChangeInputMode(InputMode.Character);
            GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", Locator.GetPlayerCamera());
            Locator.GetActiveCamera().mainCamera.enabled = true;
            Camera.enabled = false;
            Mode = CameraMode.FirstPerson;
        }
    }
}
