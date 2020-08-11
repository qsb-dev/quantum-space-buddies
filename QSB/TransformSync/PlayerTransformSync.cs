using QSB.Animation;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        private Transform _originalBody;

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        protected override uint PlayerId => netId.Value - 0;

        private Transform GetPlayerModel()
        {
            var body = Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
            _originalBody = Instantiate(body);
            _originalBody.gameObject.SetActive(false);
            return body;
        }

        private Transform GetPlayerModelCopy()
        {
            var copy = Instantiate(LocalInstance._originalBody);
            copy.gameObject.SetActive(true);
            return copy;
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetPlayerModel();

            Player.AnimationSync = GetComponent<AnimationSync>();
            Player.AnimationSync.InitLocal(body);

            Player.Body = body.gameObject;

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = GetPlayerModelCopy();

            Player.AnimationSync = GetComponent<AnimationSync>();
            Player.AnimationSync.InitRemote(body);

            var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
            marker.Init(Player);

            Player.Body = body.gameObject;

            return body;
        }

        protected override bool IsReady => Locator.GetPlayerTransform() != null && Player != null;
    }
}
