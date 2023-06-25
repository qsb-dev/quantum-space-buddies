using UnityEngine;

namespace QSB.PoolSync;

internal class CustomNomaiRemoteCamera : MonoBehaviour
{
	public OWCamera _camera;
	private AudioListener _audioListener;
	private NomaiViewerImageEffect _viewerImageEffect;
	private CustomNomaiRemoteCameraPlatform _owningPlatform;
	private CustomNomaiRemoteCameraPlatform _controllingPlatform;
	private OWCamera _controllingCamera;

	private void Awake()
	{
		_camera = GetComponent<OWCamera>();
		_audioListener = GetComponent<AudioListener>();
		_viewerImageEffect = _camera.GetComponent<NomaiViewerImageEffect>();
		_owningPlatform = GetComponentInParent<CustomNomaiRemoteCameraPlatform>();
		enabled = false;
	}

	private void OnEnable()
	{
		_camera.enabled = true;
		_audioListener.enabled = true;
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", _camera);
	}

	private void OnDisable()
	{
		_camera.enabled = false;
		_audioListener.enabled = false;
	}

	private void LateUpdate()
	{
		if (_owningPlatform == null)
		{
			_owningPlatform = GetComponentInParent<CustomNomaiRemoteCameraPlatform>();
		}

		if (_owningPlatform != null && _controllingPlatform != null)
		{
			transform.position = CustomNomaiRemoteCameraPlatform.TransformPoint(_controllingCamera.transform.position, _controllingPlatform, _owningPlatform);
			transform.rotation = CustomNomaiRemoteCameraPlatform.TransformRotation(_controllingCamera.transform.rotation, _controllingPlatform, _owningPlatform);
			_camera.fieldOfView = _controllingCamera.fieldOfView;
		}
		else
		{
			enabled = false;
		}
	}

	public void Activate(CustomNomaiRemoteCameraPlatform controllingPlatform, OWCamera viewer)
	{
		_controllingPlatform = controllingPlatform;
		_controllingCamera = viewer;
		enabled = true;
	}

	public void Deactivate()
	{
		_controllingPlatform = null;
		enabled = false;
	}

	public bool IsActive()
		=> enabled;

	public void SetImageEffectFade(float fade)
	{
		if (_viewerImageEffect != null)
		{
			_viewerImageEffect.SetFade(fade);
		}
	}
}