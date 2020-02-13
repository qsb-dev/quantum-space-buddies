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
            QSB.playerSectors[netId.Value] = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;

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

            // It's dumb that this is here, should be somewhere else.
            if (isServer) {
                NetworkServer.RegisterHandler(MsgType.Highest + 1, QSB.OnReceiveMessage);
            } else {
                NetworkManager.singleton.client.RegisterHandler(SectorMessage.Type, QSB.OnReceiveMessage);
            }
        }

        public void EnterSector (Sector sector) {
            var name = sector.GetName();
            if (name != Sector.Name.Unnamed && name != Sector.Name.Ship && name != Sector.Name.Sun) {
                QSB.playerSectors[netId.Value] = QSB.GetSectorByName(sector.GetName());

                SectorMessage msg = new SectorMessage();
                msg.sectorId = (int) sector.GetName();
                msg.senderId = netId.Value;
                if (isServer) {
                    NetworkServer.SendToAll(SectorMessage.Type, msg);
                } else {
                    connectionToServer.Send(SectorMessage.Type, msg);
                }
            }
        }

        void Update () {
            if (!_body) {
                return;
            }

            var sectorTransform = QSB.playerSectors[netId.Value];
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
