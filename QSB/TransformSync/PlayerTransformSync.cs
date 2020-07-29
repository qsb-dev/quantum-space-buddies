using QSB.Animation;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        public Transform bodyTransform;

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        private uint GetAttachedNetId()
        {
            /*
            Players are stored in PlayerRegistry using a specific ID. This ID has to remain the same
            for all components of a player, so I've chosen to used the netId of PlayerTransformSync.
            This is minus 0 so all transformsyncs follow the same template.
            */
            return netId.Value - 0;
        }

        private Transform GetPlayerModel()
        {
            return Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
        }

        protected override Transform InitLocalTransform()
        {
            var body = GetPlayerModel();

            bodyTransform = body;

            GetComponent<AnimationSync>().InitLocal(body);

            PlayerRegistry.RegisterPlayerBody(GetAttachedNetId(), body.gameObject);

            return body;
        }

        protected override Transform InitRemoteTransform()
        {
            var body = Instantiate(GetPlayerModel());

            bodyTransform = body;

            GetComponent<AnimationSync>().InitRemote(body);

            var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
            marker.SetId(netId.Value);

            PlayerRegistry.RegisterPlayerBody(GetAttachedNetId(), body.gameObject);

            return body;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null && PlayerRegistry.PlayerExists(GetAttachedNetId());
        }
    }
}
