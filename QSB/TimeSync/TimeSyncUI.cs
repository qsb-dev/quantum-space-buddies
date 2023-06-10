using Mirror;
using OWML.Common;
using QSB.Localization;
using QSB.Utility;
using QSB.WorldSync;
using System;
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

		var langController = QSBWorldSync.GetUnityObject<PauseMenuManager>().transform.GetChild(0).GetComponent<FontAndLanguageController>();
		langController.AddTextElement(_text);
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
			DebugLog.ToConsole("Error - Tried to start time sync UI when not in universe!", MessageType.Error);
			return;
		}

		_currentType = type;
		_currentReason = reason;
		_startTime = (float)NetworkTime.localTime;
		enabled = true;
		_canvas.enabled = true;
		Canvas.willRenderCanvases += OnWillRenderCanvases;

		// silly hack that shouldnt be in the ui component but oh well
		Locator.GetPlayerTransform().GetComponent<PlayerResources>()._invincible = true;
		Locator.GetDeathManager()._invincible = true;
		var shipTransform = Locator.GetShipTransform();
		if (shipTransform)
		{
			shipTransform.GetComponentInChildren<ShipDamageController>()._invincible = true;
		}
	}

	private void EndTimeSync()
	{
		_currentType = TimeSyncType.None;
		enabled = false;
		_canvas.enabled = false;
		Canvas.willRenderCanvases -= OnWillRenderCanvases;

		// silly hack that shouldnt be in the ui component but oh well
		Locator.GetPlayerTransform().GetComponent<PlayerResources>()._invincible = false;
		Locator.GetDeathManager()._invincible = false;
		var shipTransform = Locator.GetShipTransform();
		if (shipTransform)
		{
			shipTransform.GetComponentInChildren<ShipDamageController>()._invincible = false;
		}
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
						var totalSeconds = Mathf.Max(TargetTime - (float)NetworkTime.localTime, 0f);
						var minutes = Mathf.FloorToInt(totalSeconds / 60f);
						var seconds = Mathf.FloorToInt(totalSeconds) % 60;
						var milliseconds = totalSeconds % 1 * 1000;
						text = string.Format(QSBLocalization.Current.TimeSyncTooFarBehind, $"{minutes:D2}:{seconds:D2}.{milliseconds:000}");
						break;
				}

				break;

			case TimeSyncType.Pausing:
				switch ((PauseReason)_currentReason)
				{
					case PauseReason.ServerNotStarted:
						text = QSBLocalization.Current.TimeSyncWaitingForStartOfServer;
						break;

					case PauseReason.TooFarAhead:
						var totalSeconds = Mathf.Max((float)NetworkTime.localTime - TargetTime, 0f);
						var minutes = Mathf.FloorToInt(totalSeconds / 60f);
						var seconds = Mathf.FloorToInt(totalSeconds) % 60;
						var milliseconds = totalSeconds % 1 * 1000;
						text = string.Format(QSBLocalization.Current.TimeSyncTooFarAhead, $"{minutes:D2}:{seconds:D2}.{milliseconds:000}");
						break;

					case PauseReason.WaitingForAllPlayersToBeReady:
						text = QSBLocalization.Current.TimeSyncWaitForAllToReady;
						break;

					case PauseReason.WaitingForAllPlayersToDie:
						text = QSBLocalization.Current.TimeSyncWaitForAllToDie;
						break;
				}

				break;
		}

		_text.text = text;
	}
}
