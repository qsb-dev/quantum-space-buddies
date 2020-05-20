using UnityEngine;

namespace QSB.TransformSync
{
    public class ShipTransformSync : TransformSync
    {
        public static ShipTransformSync LocalInstance { get; private set; }

        private Transform GetShipModel()
        {
            return Locator.GetShipTransform();
        }

        protected override Transform InitLocalTransform()
        {
            LocalInstance = this;
            return GetShipModel().Find("Module_Cockpit/Geo_Cockpit/Cockpit_Geometry/Cockpit_Exterior");
        }

        protected override Transform InitRemoteTransform()
        {
            var shipModel = GetShipModel();

            var remoteTransform = new GameObject().transform;

            Instantiate(shipModel.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Geometry/Cockpit_Exterior"), remoteTransform);
            Instantiate(shipModel.Find("Module_Cabin/Geo_Cabin/Cabin_Geometry/Cabin_Exterior"), remoteTransform);
            Instantiate(shipModel.Find("Module_Supplies/Geo_Supplies/Supplies_Geometry/Supplies_Exterior"), remoteTransform);
            Instantiate(shipModel.Find("Module_Engine/Geo_Engine/Engine_Geometry/Engine_Exterior"), remoteTransform);

            var landingGearFront = Instantiate(shipModel.Find("Module_LandingGear/LandingGear_Front/Geo_LandingGear_Front"), remoteTransform);
            var landingGearLeft = Instantiate(shipModel.Find("Module_LandingGear/LandingGear_Left/Geo_LandingGear_Left"), remoteTransform);
            var landingGearRight = Instantiate(shipModel.Find("Module_LandingGear/LandingGear_Right/Geo_LandingGear_Right"), remoteTransform);

            Destroy(landingGearFront.Find("LandingGear_FrontCollision").gameObject);
            Destroy(landingGearLeft.Find("LandingGear_LeftCollision").gameObject);
            Destroy(landingGearRight.Find("LandingGear_RightCollision").gameObject);

            landingGearFront.localPosition
                = landingGearLeft.localPosition
                = landingGearRight.localPosition
                += Vector3.up * 3.762f;

            var physicsBody = new GameObject("ShipBodySync");

            var collider = physicsBody.AddComponent<SphereCollider>();
            collider.radius = 5;
            collider.center = Vector3.up * 5;

            var rigidBodySync = physicsBody.AddComponent<RigidbodySync>();
            rigidBodySync.Init<ShipBody>(remoteTransform);

            // TODO: If we disable remote player collisions while they are inside the ship,
            // this wouldn't be necessary. For that, we'll need to broadcast a message
            // that signals when a player is inside the ship.
            rigidBodySync.IgnoreCollision(Locator.GetPlayerTransform().gameObject);

            return remoteTransform;
        }

        protected override bool IsReady()
        {
            return GetShipModel() != null;
        }
    }
}
