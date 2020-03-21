using QSB.Animation;
using UnityEngine;

namespace QSB.TransformSync
{
    public class PlayerTransformSync : TransformSync
    {
        public static PlayerTransformSync LocalInstance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        private Transform GetPlayerModel()
        {
            return Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2");
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

            var physicsBody = new GameObject("PlayerBodySync");

            var collider = physicsBody.AddComponent<CapsuleCollider>();
            collider.radius = 1;
            collider.height = 2;
            collider.center = Vector3.up * 1;

            var rigidBodySync = physicsBody.AddComponent<RigidbodySync>();
            rigidBodySync.Init<PlayerBody>(body);
            rigidBodySync.IgnoreCollision(Locator.GetShipTransform().gameObject);

            return body;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }

    }
}
