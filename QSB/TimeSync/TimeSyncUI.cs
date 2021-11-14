﻿using OWML.Utils;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.TimeSync
{
	internal class TimeSyncUI : MonoBehaviour
	{
		public static TimeSyncUI Instance;

		public static float TargetTime;

		private Canvas _canvas;
		private Text _text;
		private float _startTime;
		private bool _isSetUp;
		private TimeSyncType _currentType;
		private Enum _currentReason;

		public void Awake()
		{
			Instance = this;
			enabled = false;

			QSBSceneManager.OnUniverseSceneLoaded += OnUniverseSceneLoad;
		}

		private void OnUniverseSceneLoad(OWScene oldScene, OWScene newScene)
		{
			_isSetUp = true;
			var obj = QSBWorldSync.GetUnityObjects<SleepTimerUI>().First();
			_canvas = obj.GetValue<Canvas>("_canvas");
			_text = obj.GetValue<Text>("_text");
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
			QSBCore.UnityEvents.RunWhen(() => Instance._isSetUp, () => Instance.StartTimeSync(type, reason));

		public static void Stop() =>
			QSBCore.UnityEvents.RunWhen(() => Instance._isSetUp, () => Instance.EndTimeSync());

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

			var totalSeconds = Mathf.Max(TargetTime - Time.timeSinceLevelLoad, 0f);
			var minutes = Mathf.FloorToInt(totalSeconds / 60f);
			var seconds = Mathf.FloorToInt(totalSeconds) % 60;
			var milliseconds = totalSeconds % 1 * 1000;
			var text = "";
			switch (_currentType)
			{
				case TimeSyncType.Fastforwarding:
					switch ((FastForwardReason)_currentReason)
					{
						case FastForwardReason.TooFarBehind:
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
							text = "Pausing to match server time...";
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
}
