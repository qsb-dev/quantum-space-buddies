using QSB.Player;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.RespawnSync
{
	public class RespawnHUDMarker : HUDDistanceMarker
	{
		private bool _isReady;

		public override void InitCanvasMarker()
		{
			_markerRadius = 0.2f;
			_markerTarget = transform;
			_markerLabel = "RESPAWN PLAYER";
			_isReady = true;

			base.InitCanvasMarker();
		}

		private void Update()
		{
			if (!_isReady)
			{
				return;
			}

			if (_canvasMarker != null)
			{
				var isVisible = _canvasMarker.IsVisible();

				if (RespawnManager.Instance.RespawnNeeded != isVisible)
				{
					_isVisible = RespawnManager.Instance.RespawnNeeded;
					_canvasMarker.SetVisibility(_isVisible);
				}
			}

			if (_isVisible && _canvasMarker != null)
			{
				var color = (Mathf.Sin(Time.unscaledTime * 10f) > 0f)
					? Color.white
					: new Color(1f, 1f, 1f, 0.1f);
				_canvasMarker._mainTextField.color = color;
				_canvasMarker._offScreenIndicator._textField.color = color;
			}
		}
	}
}
