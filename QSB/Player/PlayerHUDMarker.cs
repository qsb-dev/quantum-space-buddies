using QSB.Utility;
using UnityEngine;

namespace QSB.Player;

public class PlayerHUDMarker : HUDDistanceMarker
{
	private PlayerInfo _player;
	private bool _needsInitializing;
	private bool _isReady;

	public override void InitCanvasMarker()
	{
		_markerRadius = 2f;

		_markerTarget = new GameObject().transform;
		_markerTarget.parent = transform;

		_markerTarget.localPosition = Vector3.up * 0.25f;
	}

	public void Init(PlayerInfo player)
	{
		_player = player;
		_player.HudMarker = this;
		_needsInitializing = true;
	}

	private bool ShouldBeVisible()
	{ 
		if (_player == null)
		{
			return false;
		}

		return _player.IsReady && !_player.IsDead && (!_player.InDreamWorld || QSBPlayerManager.LocalPlayer.InDreamWorld) && _player.Visible;
	}

	private void Update()
	{
		if (_needsInitializing)
		{
			Initialize();
		}

		if (!_isReady || !_player.IsReady)
		{
			return;
		}

		if (_canvasMarker != null)
		{
			var isVisible = _canvasMarker.IsVisible();

			if (ShouldBeVisible() != isVisible)
			{
				_canvasMarker.SetVisibility(ShouldBeVisible());
			}
		}
		else
		{
			DebugLog.ToConsole($"Warning - _canvasMarker for {_player} is null!", OWML.Common.MessageType.Warning);
		}
	}

	private void Initialize()
	{
		if (_player.Name == null)
		{
			DebugLog.ToConsole($"Error - {_player} has a null name!", OWML.Common.MessageType.Error);
			_player.Name = "NULL";
		}

		_markerLabel = _player.Name.ToUpper();
		_needsInitializing = false;
		_isReady = true;

		base.InitCanvasMarker();
	}

	public void Remove()
	{
		_isReady = false;
		// do N O T destroy the parent - it completely breaks the ENTIRE GAME
		if (_canvasMarker != null)
		{
			_canvasMarker.DestroyMarker();
		}

		if (_markerTarget != null)
		{
			Destroy(_markerTarget.gameObject);
		}

		Destroy(this);
	}
}