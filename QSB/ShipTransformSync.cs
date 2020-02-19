using System;
using UnityEngine;

namespace QSB
{
    class ShipTransformSync : TransformSync
    {
        Transform _shipModel;

        Transform GetShipModel()
        {
            if (!_shipModel)
            {
                // TODO: specify ship model
                _shipModel = Locator.GetShipBody().transform;
            }
            return _shipModel;
        }

        protected override Transform GetLocalTransform()
        {
            return GetShipModel();
        }

        protected override Transform GetRemoteTransform()
        {
            var shipModel = GetShipModel();
            var cockpit = Instantiate(shipModel.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Geometry/Cockpit_Exterior"));
            var cabin = Instantiate(shipModel.Find("Module_Cabin/Geo_Cabin/Cabin_Geometry/Cabin_Exterior"));
            var supplies = Instantiate(shipModel.Find("Module_Supplies/Geo_Supplies/Supplies_Geometry/Supplies_Exterior"));
            var engine = Instantiate(shipModel.Find("Module_Engine/Geo_Engine/Engine_Geometry/Engine_Exterior"));
            //var landingGear = shipModel.Find("Geo_LandingGear_Front");

            var remoteTransform = new GameObject().transform;

            cockpit.parent = cabin.parent = supplies.parent = engine.parent = remoteTransform;

            return remoteTransform;
        }
    }
}
