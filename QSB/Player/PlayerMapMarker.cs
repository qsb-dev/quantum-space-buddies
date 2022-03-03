using UnityEngine;

namespace QSB.Player;

public class PlayerMapMarker : MonoBehaviour
{
	public string PlayerName;
	private Transform _playerTransform;
	private CanvasMapMarker _canvasMarker;
	private bool _canvasMarkerInitialized;

	public void Awake()
	{
		GlobalMessenger.AddListener("EnterMapView", new Callback(OnEnterMapView));
		GlobalMessenger.AddListener("ExitMapView", new Callback(OnExitMapView));
	}

	public void Start()
	{
		enabled = false;
		_playerTransform = Locator.GetPlayerTransform();
	}

	public void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", new Callback(OnEnterMapView));
		GlobalMessenger.RemoveListener("ExitMapView", new Callback(OnExitMapView));
	}

	public void InitMarker()
	{
		var obj = GameObject.FindWithTag("MapCamera");
		var markerManager = obj.GetRequiredComponent<MapController>().GetMarkerManager();
		_canvasMarker = markerManager.InstantiateNewMarker(true);
		var component = GetComponent<OWRigidbody>();
		if (component != null)
		{
			markerManager.RegisterMarker(_canvasMarker, component);
		}
		else
		{
			markerManager.RegisterMarker(_canvasMarker, transform);
		}

		_canvasMarker.SetLabel(PlayerName.ToUpper());
		_canvasMarker.SetColor(Color.white);
		_canvasMarker.SetVisibility(false);
		_canvasMarkerInitialized = true;
	}

	private void OnEnterMapView() => enabled = true;
	private void OnExitMapView() => enabled = false;

	public void LateUpdate()
	{
		if (!_canvasMarkerInitialized)
		{
			InitMarker();
		}

		var a = Locator.GetActiveCamera().WorldToScreenPoint(transform.position);
		var b = Locator.GetActiveCamera().WorldToScreenPoint(_playerTransform.position);
		var vector = a - b;
		vector.z = 0f;
		var flag = a.z > 0f;
		if (_canvasMarker.IsVisible() != flag)
		{
			_canvasMarker.SetVisibility(flag);
		}
	}
}