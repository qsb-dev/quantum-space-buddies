using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.Events;
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

            // TODO: If we disable remote player collisions while they are inside the ship,
            // this wouldn't be necessary. For that, we'll need to broadcast a message
            // that signals when a player is inside the ship.
            rigidBodySync.IgnoreCollision(Locator.GetShipTransform().gameObject);

            var marker = body.gameObject.AddComponent<PlayerHUDMarker>();
            marker.SetId(netId.Value);

            return body;
        }

        protected override bool IsReady()
        {
            return Locator.GetPlayerTransform() != null;
        }

    }
}
