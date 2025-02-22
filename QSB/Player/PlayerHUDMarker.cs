using QSB.ServerSettings;
using QSB.Utility;
using UnityEngine;

namespace QSB.Player;

[UsedInUnityProject]
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

		if (!ServerSettingsManager.ShowPlayerNames)
		{
			return false;
		}

		return _player.IsReady &&
		       !_player.IsDead &&
		       _player.Visible &&
		       _player.InDreamWorld == QSBPlayerManager.LocalPlayer.InDreamWorld && // TODO: check for same dreamworld zone
		       _player.IsInMoon == QSBPlayerManager.LocalPlayer.IsInMoon &&
		       !_player.IsInBramble && // TODO: players in the same bramble node should be able to see their markers
		       ShouldBeVisibleEye();
	}

	private bool ShouldBeVisibleEye()
	{
		if (QSBSceneManager.CurrentScene != OWScene.EyeOfTheUniverse)
		{
			return true;
		}

		var localPlayer = QSBPlayerManager.LocalPlayer;
		var localState = localPlayer.EyeState;

		if (localState 
			    is EyeState.AboardVessel 
			    or EyeState.WarpedToSurface 
		    && _player.EyeState 
			    is EyeState.AboardVessel 
			    or EyeState.WarpedToSurface)
		{
			return true;
		}

		if (localState == EyeState.Observatory && _player.EyeState == EyeState.Observatory)
		{
			return true;
		}

		if (localState
			    is EyeState.ForestOfGalaxies
			    or EyeState.ForestIsDark
		    && _player.EyeState
				is EyeState.ForestOfGalaxies
				or EyeState.ForestIsDark)
		{
			return true;
		}

		if (localState
			    is EyeState.InstrumentHunt
			    or EyeState.JamSession
		    && _player.EyeState
			    is EyeState.InstrumentHunt
				or EyeState.JamSession)
		{
			return true;
		}

		return false;
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