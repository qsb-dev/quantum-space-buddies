using QSB.Animation;
using QSB.Utility;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        static PlayerTransformSync()
        {
            DebugLog.ToConsole("constructor of playertransformsync");
            AnimControllerPatch.Init();
        }

        public override void OnNetworkDestroy()
        {
            DebugLog.ToConsole("PlayerTransformSync on network destroy");
            base.OnNetworkDestroy();
        }

        public override void OnStartLocalPlayer()
        {
            DebugLog.ToConsole("onstartlocalplayer of playertransformsync, id of " + PlayerId);
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

        public override bool IsReady => Locator.GetPlayerTransform() != null && Player != null && Player.IsReady;
    }
}
