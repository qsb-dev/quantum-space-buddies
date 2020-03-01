using UnityEngine;

namespace QSB.TransformSync
{
    public class ShipTransformSync : TransformSync
    {
        public static ShipTransformSync LocalInstance { get; private set; }

        private Transform _shipModel;

        private Transform GetShipModel()
        {
            if (!_shipModel)
            {
                _shipModel = Locator.GetShipBody().transform;
            }
            return _shipModel;
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

            var physicsBody = new GameObject();

            var collider = physicsBody.AddComponent<SphereCollider>();
            collider.radius = 5;
            collider.center = Vector3.up * 5;

            var rigidBodySync = physicsBody.AddComponent<RigidbodySync>();
            rigidBodySync.target = remoteTransform;

            //var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            //sphere.parent = rigidBodySync.transform;
            //sphere.localScale = Vector3.one * 10;
            //sphere.localRotation = Quaternion.identity;
            //sphere.localPosition = Vector3.up * 5;
            //Destroy(sphere.GetComponent<BoxCollider>());

            return remoteTransform;
        }
    }
}
