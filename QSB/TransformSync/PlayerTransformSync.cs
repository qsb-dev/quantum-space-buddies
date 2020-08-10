using QSB.Animation;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        private Transform _bodyBlueprint;

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        protected override uint PlayerId => netId.Value - 0;

        private Transform GetPlayerModel()
        {
            var body = Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
            StoreBlueprintIfMissing(body);
            return body;
        }

        private Transform GetPlayerModelCopy()
        {
            StoreBlueprintIfMissing(Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2"));
            return GetBlueprintCopy();
        }

        private void StoreBlueprintIfMissing(Transform body)
        {
            if (_bodyBlueprint == null)
            {
                _bodyBlueprint = Instantiate(body);
                _bodyBlueprint.gameObject.SetActive(false);
            }
        }

        private Transform GetBlueprintCopy()
        {
            var copy = Instantiate(_bodyBlueprint);
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
