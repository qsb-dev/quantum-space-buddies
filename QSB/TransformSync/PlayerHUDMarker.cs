using QSB.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.TransformSync
{
    class PlayerHUDMarker : HUDDistanceMarker
    {
        private uint _netId = uint.MaxValue;
        private bool _isReady;

        protected override void InitCanvasMarker()
        {
            _markerRadius = 2f;

            _markerTarget = new GameObject().transform;
            _markerTarget.parent = transform;

            _markerTarget.localPosition = Vector3.zero;
        }

        public void SetId(uint netId)
        {
            _netId = netId;
            _isReady = true;
        }

        protected override void RefreshOwnVisibility()
        {
            if (_canvasMarker != null)
            {
                _canvasMarker.SetVisibility(true);
            }
        }

        void Update()
        {
            if (_isReady && PlayerRegistry.IsPlayerReady(_netId))
            {
                _markerLabel = PlayerRegistry.GetPlayerName(_netId);
                _isReady = false;

                base.InitCanvasMarker();
            }
        }
    }
}
