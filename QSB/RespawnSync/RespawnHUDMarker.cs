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
			DebugLog.DebugWrite($"InitCanvasMarker");

			_markerRadius = 2f;

			_markerTarget = new GameObject().transform;
			_markerTarget.parent = transform;

			_markerTarget.localPosition = Vector3.up * 0.25f;
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
					DebugLog.DebugWrite($"set visibility to {isVisible}");
					_canvasMarker.SetVisibility(RespawnManager.Instance.RespawnNeeded);
				}
			}
		}

		public void Initialize()
		{
			DebugLog.DebugWrite($"initialize");
			_markerLabel = "RESPAWN PLAYER";
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
}
