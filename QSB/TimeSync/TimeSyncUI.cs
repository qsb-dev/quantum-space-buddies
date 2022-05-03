﻿using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.TimeSync;

internal class TimeSyncUI : MonoBehaviour, IAddComponentOnStart
{
	private static TimeSyncUI _instance;

	public static float TargetTime;

	private Canvas _canvas;
	private Text _text;
	private float _startTime;
	private bool _isSetUp;
	private TimeSyncType _currentType;
	private Enum _currentReason;

	private void Awake()
	{
		_instance = this;
		enabled = false;

		QSBSceneManager.OnUniverseSceneLoaded += OnUniverseSceneLoad;
	}

	private void OnUniverseSceneLoad(OWScene oldScene, OWScene newScene)
	{
		_isSetUp = true;
		var obj = QSBWorldSync.GetUnityObject<SleepTimerUI>();
		_canvas = obj._canvas;
		_text = obj._text;
		_canvas.enabled = false;
	}

	public void OnDestroy()
	{
		QSBSceneManager.OnUniverseSceneLoaded -= OnUniverseSceneLoad;
		if (_canvas != null && _canvas.enabled)
		{
			Canvas.willRenderCanvases -= OnWillRenderCanvases;
		}
	}

	public static void Start(TimeSyncType type, Enum reason) =>
		Delay.RunWhen(() => _instance._isSetUp, () => _instance.StartTimeSync(type, reason));

	public static void Stop() =>
		Delay.RunWhen(() => _instance._isSetUp, () => _instance.EndTimeSync());

	private void StartTimeSync(TimeSyncType type, Enum reason)
	{
		if (!QSBSceneManager.IsInUniverse)
		{
			DebugLog.ToConsole("Error - Tried to start time sync UI when not in universe!", OWML.Common.MessageType.Error);
			return;
		}

		_currentType = type;
		_currentReason = reason;
		_startTime = Time.timeSinceLevelLoad;
		enabled = true;
		_canvas.enabled = true;
		Canvas.willRenderCanvases += OnWillRenderCanvases;
	}

	private void EndTimeSync()
	{
		_currentType = TimeSyncType.None;
		enabled = false;
		_canvas.enabled = false;
		Canvas.willRenderCanvases -= OnWillRenderCanvases;
	}

	private void OnWillRenderCanvases()
	{
		if (!_isSetUp)
		{
			return;
		}

		var text = "";
		switch (_currentType)
		{
			case TimeSyncType.FastForwarding:
				switch ((FastForwardReason)_currentReason)
				{
					case FastForwardReason.TooFarBehind:
						var totalSeconds = Mathf.Max(TargetTime - Time.timeSinceLevelLoad, 0f);
						var minutes = Mathf.FloorToInt(totalSeconds / 60f);
						var seconds = Mathf.FloorToInt(totalSeconds) % 60;
						var milliseconds = totalSeconds % 1 * 1000;
						text = $"{minutes:D2}:{seconds:D2}.{milliseconds:000}"
						       + Environment.NewLine
						       + "Fast-forwarding to match server time...";
						break;
				}

				break;

			case TimeSyncType.Pausing:
				switch ((PauseReason)_currentReason)
				{
					case PauseReason.ServerNotStarted:
						text = "Waiting for server to start...";
						break;

					case PauseReason.TooFarAhead:
						var totalSeconds = Mathf.Max(Time.timeSinceLevelLoad - TargetTime, 0f);
						var minutes = Mathf.FloorToInt(totalSeconds / 60f);
						var seconds = Mathf.FloorToInt(totalSeconds) % 60;
						var milliseconds = totalSeconds % 1 * 1000;
						text = $"{minutes:D2}:{seconds:D2}.{milliseconds:000}"
						       + Environment.NewLine
						       + "Pausing to match server time...";
						break;

					case PauseReason.WaitingForAllPlayersToBeReady:
						text = "Waiting for start of loop...";
						break;

					case PauseReason.WaitingForAllPlayersToDie:
						text = "Waiting for end of loop...";
						break;
				}

				break;
		}

		_text.text = text;
	}
}