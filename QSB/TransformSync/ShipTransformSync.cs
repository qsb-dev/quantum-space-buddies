using QSB.Utility;
using UnityEngine;

namespace QSB.TransformSync
{
    public class ShipTransformSync : TransformSync
    {
        public static ShipTransformSync LocalInstance { get; private set; }

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
        }

        uint GetAttachedNetId()
        {
            /*
            Players are stored in PlayerRegistry using a specific ID. This ID has to remain the same
            for all components of a player, so I've chosen to used the netId of PlayerTransformSync.
            Since every networkbehaviour has it's own ascending netId, and we know that PlayerCameraSync
            is the 2nd network transform to be loaded (After PlayerTransformSync), we can just
            minus 1 from ShipTransformSync's netId to get PlayerTransformSyncs's netId.
            */
            return netId.Value - 1;
        }

        private Transform GetShipModel()
        {
            return Locator.GetShipTransform();
        }

        protected override Transform InitLocalTransform()
        {
            DebugLog.ToConsole($"Local ShipTransformSync for id {GetAttachedNetId()}");
            return GetShipModel().Find("Module_Cockpit/Geo_Cockpit/Cockpit_Geometry/Cockpit_Exterior");
        }

        protected override Transform InitRemoteTransform()
        {
            DebugLog.ToConsole($"Remote ShipTransformSync for id {GetAttachedNetId()}");
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

        protected override bool IsReady()
        {
            return GetShipModel() != null;
        }
    }
}
