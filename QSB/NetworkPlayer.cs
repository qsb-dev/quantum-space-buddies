using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class NetworkPlayer: NetworkBehaviour {
        Transform _body;
        float _smoothSpeed = 10f;
        public static NetworkPlayer localInstance { get; private set; }
        Transform _sectorTransform;

        void Start () {
            QSB.LogToScreen("Started network player");

            _sectorTransform = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;

            var player = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");
            if (isLocalPlayer) {
                localInstance = this;
                _body = player;
            } else {
                _body = Instantiate(player);
                _body.GetComponent<PlayerAnimController>().enabled = false;
                _body.Find("player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head").gameObject.layer = 0;
                _body.parent = transform;
                _body.localPosition = Vector3.zero;
                _body.localRotation = Quaternion.identity;
            }

            if (isServer) {
                QSB.LogToScreen("Register Handler Server");
                NetworkServer.RegisterHandler(MsgType.Highest + 1, OnReceiveMessage);
            } else {
                QSB.LogToScreen("Register Handler Client");
                NetworkManager.singleton.client.RegisterHandler(MsgType.Highest + 1, OnReceiveMessage);
            }
        }

        private void OnReceiveMessage (NetworkMessage netMsg) {
            SectorMessage msg = netMsg.ReadMessage<SectorMessage>();
            QSB.LogToScreen("Messager Receive", msg.senderId.ToString(), netId.Value.ToString());

            if (isServer) {
                if (msg.senderId == netId.Value) {
                    NetworkServer.SendToAll(SectorMessage.Type, msg);
                    SetSectorById(msg.sectorId);
                }
            } else {
                if (msg.senderId == netId.Value) {
                    SetSectorById(msg.sectorId);
                }
            }
        }

        void SetSectorById (int sectorId) {
            var sectorName = (Sector.Name) sectorId;
            var sectors = GameObject.FindObjectsOfType<Sector>();
            foreach (var sector in sectors) {
                if (sectorName == sector.GetName()) {
                    _sectorTransform = sector.transform;
                    return;
                }
            }
        }

        public void EnterSector (Sector sector) {
            var name = sector.GetName();
            if (name != Sector.Name.Unnamed && name != Sector.Name.Ship) {
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
            if (isLocalPlayer) {
                transform.position = _body.position - _sectorTransform.position;
                transform.rotation = _body.rotation * Quaternion.Inverse(_sectorTransform.rotation);
            } else {
                _body.position = Vector3.Lerp(_body.position, _sectorTransform.position + transform.position, _smoothSpeed * Time.deltaTime);
                _body.rotation = transform.rotation * _sectorTransform.rotation;
            }
        }
    }
}
