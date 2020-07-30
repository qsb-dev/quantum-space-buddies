using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerHUDMarker : HUDDistanceMarker
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

        private void Update()
        {
            if (!_isReady || !PlayerRegistry.GetPlayer(_netId).IsReady)
            {
                return;
            }
            _markerLabel = PlayerRegistry.GetPlayer(_netId).Name;
            _isReady = false;

            base.InitCanvasMarker();
        }
    }
}
