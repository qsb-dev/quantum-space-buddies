using QSB.Utility;
using UnityEngine;

namespace QSB.Player;

[UsedInUnityProject]
public class PlayerMapMarker : MonoBehaviour
{
	private PlayerInfo _player;
	private CanvasMapMarker _canvasMarker;
	private bool _canvasMarkerInitialized;
	private bool _hasBeenSetUpForInit;

	public void Awake()
	{
		GlobalMessenger.AddListener("EnterMapView", new Callback(OnEnterMapView));
		GlobalMessenger.AddListener("ExitMapView", new Callback(OnExitMapView));
	}

	public void Start()
	{
		enabled = false;
	}

	public void OnDestroy()
	{
		GlobalMessenger.RemoveListener("EnterMapView", new Callback(OnEnterMapView));
		GlobalMessenger.RemoveListener("ExitMapView", new Callback(OnExitMapView));
	}

	public void Init(PlayerInfo player)
	{
		_player = player;
		_hasBeenSetUpForInit = true;
	}

	public void InitMarker()
	{
		var obj = GameObject.FindWithTag("MapCamera");
		var markerManager = obj.GetRequiredComponent<MapController>().GetMarkerManager();
		_canvasMarker = markerManager.InstantiateNewMarker(true);
		markerManager.RegisterMarker(_canvasMarker, transform);

		_canvasMarker.SetLabel(_player.Name.ToUpper());
		_canvasMarker.SetColor(Color.white);
		_canvasMarker.SetVisibility(false);
		_canvasMarkerInitialized = true;
	}

	private void OnEnterMapView() => enabled = true;
	private void OnExitMapView() => enabled = false;

	private bool ShouldBeVisible()
	{
		if (_player == null)
		{
			return false;
		}

		var playerScreenPos = Locator.GetActiveCamera().WorldToScreenPoint(transform.position);
		var isInfrontOfCamera = playerScreenPos.z > 0f;

		return isInfrontOfCamera &&
			_player.IsReady &&
			!_player.IsDead &&
			_player.Visible &&
			_player.InDreamWorld == QSBPlayerManager.LocalPlayer.InDreamWorld &&
			_player.IsInMoon == QSBPlayerManager.LocalPlayer.IsInMoon;
	}

	public void LateUpdate()
	{
		if (!_canvasMarkerInitialized && _hasBeenSetUpForInit)
		{
			InitMarker();
		}

		var shouldBeVisible = ShouldBeVisible();
		
		if (_canvasMarker.IsVisible() != shouldBeVisible)
		{
			_canvasMarker.SetVisibility(shouldBeVisible);
		}
	}
}