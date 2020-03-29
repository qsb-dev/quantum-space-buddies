using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.TransformSync
{
    class PlayerHUDMarker : HUDDistanceMarker
    {
        protected override void InitCanvasMarker()
        {
            _markerLabel = "Player";
            _markerRadius = 0.3f;

            _markerTarget = new GameObject().transform;
            _markerTarget.parent = transform;
            // I'm not really sure why this has to be 20 instead of 2 (the player height in units).
            // But that's the only way it looks right.
            _markerTarget.localPosition = Vector3.up * 20;

            base.InitCanvasMarker();
        }

        protected override void RefreshOwnVisibility()
        {
            if (_canvasMarker != null)
            {
                _canvasMarker.SetVisibility(true);
            }
        }
    }
}
