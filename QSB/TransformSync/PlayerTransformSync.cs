using QSB.Animation;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        static PlayerTransformSync()
        {
            AnimControllerPatch.Init();
        }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        public override uint PlayerId => netId.Value - 0;

        private Transform GetPlayerModel()
        {
            return Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetPlayerModel();

            GetComponent<AnimationSync>().InitLocal(body);

            Player.Body = body.gameObject;

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetPlayerModel());

            GetComponent<AnimationSync>().InitRemote(body);

            var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
            marker.Init(Player);

            Player.Body = body.gameObject;

            return body;
        }

        protected override bool IsReady => Locator.GetPlayerTransform() != null && Player != null;
    }
}
