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
		private Color _textColor;

		private void Awake()
		{
			Instance = this;
			enabled = false;
			_canvas.enabled = false;
			_textColor = _text.color;
		}

		private void OnDestroy()
		{
			if (_canvas.enabled)
			{
				Canvas.willRenderCanvases -= OnWillRenderCanvases;
			}
		}

		public static void Start() => Instance.StartFastForward();
		public static void Stop() => Instance.EndFastForward();

		private void StartFastForward()
		{
			_startTime = Time.timeSinceLevelLoad;
			_startTimeUnscaled = Time.unscaledTime;
			enabled = true;
			_canvas.enabled = true;
			_text.text = "00:00";
			_text.color = new Color(_textColor.r, _textColor.g, _textColor.b, 0f);
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
			var totalSeconds = Mathf.Max(Time.timeSinceLevelLoad - _startTime, 0f);
			var minutes = Mathf.FloorToInt(totalSeconds / 60f);
			var seconds = Mathf.FloorToInt(totalSeconds) % 60;
			_text.text = $"{minutes.ToString("D2")}:{seconds.ToString("D2")}";
			var alpha = Mathf.Clamp01((Time.unscaledTime - _startTimeUnscaled) / 3f);
			_text.color = new Color(_textColor.r, _textColor.g, _textColor.b, alpha);
		}
	}
}