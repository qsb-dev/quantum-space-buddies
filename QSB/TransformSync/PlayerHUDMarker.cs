using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.TransformSync
{
    class PlayerHUDMarker : HUDDistanceMarker
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void InitCanvasMarker()
        {
            DebugLog.All("Start Canvas Marker");

            _markerTarget = transform;
            _markerLabel = "Player";
            _markerRadius = 0.3f;
            base.InitCanvasMarker();
            //this._canvasMarker.OnMarkerSecondaryLabelUpdate += this.OnUpdateMarkerSecondaryLabel;
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
