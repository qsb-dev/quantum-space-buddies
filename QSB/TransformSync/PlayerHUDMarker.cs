using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerHUDMarker : HUDDistanceMarker
    {
        private PlayerInfo _player;
        private bool _isReady;

        protected override void InitCanvasMarker()
        {
            _markerRadius = 2f;

            _markerTarget = new GameObject().transform;
            _markerTarget.parent = transform;

            _markerTarget.localPosition = Vector3.up * 2;
        }

        public void Init(PlayerInfo player)
        {
            _player = player;
            _player.HudMarker = this;
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
            if (!_isReady || !_player.IsReady)
            {
                return;
            }
            _markerLabel = _player.Name;
            _isReady = false;

            base.InitCanvasMarker();
        }

        public void Remove()
        {
            Destroy(transform.parent.gameObject);
        }

    }
}
