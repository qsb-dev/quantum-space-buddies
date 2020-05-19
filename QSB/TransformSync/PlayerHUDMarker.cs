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
            _markerRadius = 0.3f;

            _markerTarget = new GameObject().transform;
            _markerTarget.parent = transform;
            // I'm not really sure why this has to be 20 instead of 2 (the player height in units).
            // But that's the only way it looks right.
            _markerTarget.localPosition = Vector3.up * 20;
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
            if (_isReady && PlayerJoin.PlayerNames.ContainsKey(_netId))
            {
                _markerLabel = PlayerJoin.PlayerNames[_netId];
                _isReady = false;

                base.InitCanvasMarker();
            }
        }
    }
}
