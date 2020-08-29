using UnityEngine;

namespace QSB.TransformSync
{
    public class ShipTransformSync : QSBTransformSync
    {
        public static ShipTransformSync LocalInstance { get; private set; }

        protected override uint PlayerIdOffset => 1;

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        private Transform GetShipModel()
        {
            return Locator.GetShipTransform();
        }

        protected override Transform InitLocalTransform()
        {
            return GetShipModel().Find("Module_Cockpit/Geo_Cockpit/Cockpit_Geometry/Cockpit_Exterior");
        }

        protected override Transform InitRemoteTransform()
        {
            var shipModel = GetShipModel();

            var remoteTransform = new GameObject().transform;

            Instantiate(shipModel.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Geometry/Cockpit_Exterior"), remoteTransform);
            Instantiate(shipModel.Find("Module_Cabin/Geo_Cabin/Cabin_Geometry/Cabin_Exterior"), remoteTransform);
            Instantiate(shipModel.Find("Module_Cabin/Geo_Cabin/Cabin_Tech/Cabin_Tech_Exterior"), remoteTransform);
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

            return remoteTransform;
        }

        public override bool IsReady => GetShipModel() != null && PlayerRegistry.PlayerExists(PlayerId) && Player.IsReady;
    }
}
