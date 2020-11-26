using OWML.ModHelper.Events;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace QSB.TimeSync
{
	class FastForwardUI : MonoBehaviour
	{
		public static FastForwardUI Instance;

		private Canvas _canvas;
		private Text _text;
		private float _startTime;
		private float _startTimeUnscaled;
		private bool _isSetUp;

		private void Awake()
		{
			Instance = this;
			enabled = false;

			QSBSceneManager.OnUniverseSceneLoaded += OnUniverseSceneLoad;
		}

		private void OnUniverseSceneLoad(OWScene scene)
		{
			_isSetUp = true;
			var obj = Resources.FindObjectsOfTypeAll<SleepTimerUI>()[0];
			_canvas = obj.GetValue<Canvas>("_canvas");
			_text = obj.GetValue<Text>("_text");
			_canvas.enabled = false;
		}

		private void OnDestroy()
		{
			QSBSceneManager.OnUniverseSceneLoaded -= OnUniverseSceneLoad;
			if (_canvas.enabled)
			{
				Canvas.willRenderCanvases -= OnWillRenderCanvases;
			}
		}

		public static void Start() => QSB.Helper.Events.Unity.RunWhen(() => Instance._isSetUp, Instance.StartFastForward);
		public static void Stop() => QSB.Helper.Events.Unity.RunWhen(() => Instance._isSetUp, Instance.EndFastForward);

		private void StartFastForward()
		{
			_startTime = Time.timeSinceLevelLoad;
			_startTimeUnscaled = Time.unscaledTime;
			enabled = true;
			_canvas.enabled = true;
			_text.text = "00:00";
			Canvas.willRenderCanvases += OnWillRenderCanvases;
		}

		private void EndFastForward()
		{
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
			var totalSeconds = Mathf.Max(Time.timeSinceLevelLoad - _startTime, 0f);
			var minutes = Mathf.FloorToInt(totalSeconds / 60f);
			var seconds = Mathf.FloorToInt(totalSeconds) % 60;
			_text.text = $"{minutes.ToString("D2")}:{seconds.ToString("D2")}" 
				+ Environment.NewLine 
				+ "Fast-forwarding to match server time...";
		}
	}
}