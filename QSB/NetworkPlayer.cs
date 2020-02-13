using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class NetworkPlayer: NetworkBehaviour {
        Transform _body;
        public static NetworkPlayer localInstance { get; private set; }

        void Start () {
            QSB.Log("Start NetworkPlayer", netId.Value);
            SectorSync.SetSector(netId, Sector.Name.TimberHearth, true);

            transform.parent = Locator.GetRootTransform();

            var player = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");
            if (isLocalPlayer) {
                localInstance = this;
                _body = player;
            } else {
                _body = Instantiate(player);
                _body.GetComponent<PlayerAnimController>().enabled = false;
                _body.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head").gameObject.layer = 0;
                _body.Find("Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_Helmet").gameObject.layer = 0;
            }
        }

        public void EnterSector (Sector sector) {
            SectorSync.SetSector(netId, sector.GetName());
        }

        void Update () {
            if (!_body) {
                return;
            }

            var sectorTransform = SectorSync.GetSector(netId);
            if (isLocalPlayer) {
                transform.position = sectorTransform.InverseTransformPoint(_body.position);
                transform.rotation = sectorTransform.InverseTransformRotation(_body.rotation);
            } else {
                _body.parent = sectorTransform;
                _body.position = sectorTransform.TransformPoint(transform.position);
                _body.rotation = sectorTransform.rotation * transform.rotation;
            }
        }
    }
}
