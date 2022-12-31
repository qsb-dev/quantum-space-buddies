﻿using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace QSB.Taunts.ThirdPersonCamera;

internal class CameraManager : MonoBehaviour, IAddComponentOnStart
{
	public static CameraManager Instance;
	public bool IsSetUp { get; private set; }
	public CameraMode Mode { get; private set; }

	private GameObject _cameraBase;
	private GameObject _cameraObj;
	private Camera _camera;
	private OWCamera _owCamera;

	public void Start()
	{
		QSBSceneManager.OnUniverseSceneLoaded += (OWScene oldScene, OWScene newScene) => Delay.RunWhen(() => Locator.GetPlayerTransform() !=null,  OnSceneLoaded);
		Instance = this;
	}

	public void OnDestroy()
	{
		QSBSceneManager.OnUniverseSceneLoaded -= (OWScene oldScene, OWScene newScene) => Delay.RunWhen(() => Locator.GetPlayerTransform() != null, OnSceneLoaded);
		Instance = null;
		IsSetUp = false;
	}

	private void OnSceneLoaded()
	{
		_cameraBase = new GameObject();
		_cameraBase.SetActive(false);
		_cameraBase.transform.parent = Locator.GetPlayerTransform();
		_cameraBase.transform.localPosition = Vector3.zero;
		_cameraBase.transform.localRotation = Quaternion.Euler(0, 0, 0);

		_cameraObj = new GameObject();
		_cameraObj.transform.parent = _cameraBase.transform;
		_cameraObj.transform.localPosition = new Vector3(0, 0, -5f);
		_cameraObj.transform.localRotation = Quaternion.Euler(0, 0, 0);

		_camera = _cameraObj.AddComponent<Camera>();
		_camera.cullingMask = Locator.GetPlayerCamera().mainCamera.cullingMask;
		_camera.clearFlags = CameraClearFlags.Color;
		_camera.backgroundColor = Color.black;
		_camera.fieldOfView = 90f;
		_camera.nearClipPlane = 0.1f;
		_camera.farClipPlane = 40000f;
		_camera.depth = 0f;
		_camera.enabled = false;
		_owCamera = _cameraObj.AddComponent<OWCamera>();
		_owCamera.renderSkybox = true;

		_cameraBase.AddComponent<CameraController>().CameraObject = _cameraObj;

		var screenGrab = _cameraObj.AddComponent<FlashbackScreenGrabImageEffect>();
		screenGrab._downsampleShader = Locator.GetPlayerCamera().GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader;

		var fogImage = _cameraObj.AddComponent<PlanetaryFogImageEffect>();
		fogImage.fogShader = Locator.GetPlayerCamera().GetComponent<PlanetaryFogImageEffect>().fogShader;

		var postProcessing = _cameraObj.AddComponent<PostProcessingBehaviour>();
		postProcessing.profile = Locator.GetPlayerCamera().GetComponent<PostProcessingBehaviour>().profile;

		_cameraBase.SetActive(true);

		IsSetUp = true;

		var api = QSBCore.Helper.Interaction.TryGetModApi<ICommonCameraAPI>("xen.CommonCameraUtility");
		api.RegisterCustomCamera(_owCamera);
	}

	public void SwitchTo3rdPerson()
	{
		if (!IsSetUp)
		{
			DebugLog.ToConsole("Warning - Camera not set up!", MessageType.Warning);
			OWInput.ChangeInputMode(InputMode.None);
			Mode = CameraMode.ThirdPerson;
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
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _owCamera);
		Locator.GetPlayerCamera().mainCamera.enabled = false;
		if (_cameraObj.GetComponent<PostProcessingBehaviour>() == null)
		{
			var postProcessing = _cameraObj.AddComponent<PostProcessingBehaviour>();
			postProcessing.profile = Locator.GetPlayerCamera().gameObject.GetComponent<PostProcessingBehaviour>().profile;
		}

		_camera.enabled = true;
		Mode = CameraMode.ThirdPerson;
	}

	public void SwitchTo1stPerson()
	{
		if (!IsSetUp)
		{
			DebugLog.ToConsole("Warning - Camera not set up!", MessageType.Warning);
			OWInput.ChangeInputMode(InputMode.Character);
			Mode = CameraMode.FirstPerson;
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
		_camera.enabled = false;
		Mode = CameraMode.FirstPerson;
	}
}
