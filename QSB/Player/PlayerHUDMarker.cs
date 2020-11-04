using UnityEngine;

namespace QSB.Player
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
            _markerLabel = _player.Name.ToUpper();
            _isReady = false;

            base.InitCanvasMarker();
        }

        public void Remove()
        {
            // do N O T destroy the parent - it completely breaks the ENTIRE GAME
            if (_canvasMarker?.gameObject != null)
            {
                _canvasMarker.DestroyMarker();
            }
            Destroy(_markerTarget.gameObject);
            Destroy(this);
        }

    }
}
