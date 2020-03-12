using QSB.Animation;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        private Transform _playerModel;

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        private Transform GetPlayerModel()
        {
            if (!_playerModel)
            {
                _playerModel = Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
            }
            return _playerModel;
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetPlayerModel();

            GetComponent<AnimationSync>().InitLocal(body);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetPlayerModel());

            GetComponent<AnimationSync>().InitRemote(body);

            return body;
        }

        void Update()
        {
            if (!_isInitialized && Locator.GetPlayerTransform() != null)
            {
                DebugLog.All("######## Init Transform Sync");
                base.Init();
            }
            else if (_isInitialized && Locator.GetPlayerTransform() == null)
            {
                DebugLog.All("########### Reset Transform Sync");
                base.Reset();
            }
        }

    }
}
