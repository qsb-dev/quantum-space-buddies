using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class NetworkPlayer: NetworkBehaviour {
        Transform _body;
        float _smoothSpeed = 10f;
        public static NetworkPlayer localInstance { get; private set; }

        public Sector.Name _sector;
        Transform _sectorTransform;

        public class SectorMessage: MessageBase {
            public int sector;
            public int id;

            public override void Deserialize (NetworkReader reader) {
                sector = reader.ReadInt32();
                id = reader.ReadInt32();
            }

            public override void Serialize (NetworkWriter writer) {
                writer.Write(sector);
                writer.Write(id);
            }
        }

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
                NetworkServer.RegisterHandler(MsgType.Highest + 1, OnReceiveMessage);
            } else {
                NetworkManager.singleton.client.RegisterHandler(MsgType.Highest + 1, OnReceiveMessage);
            }

        }

        void DoThisThing () {
            _sector = Sector.Name.Comet;
        }

        private void OnReceiveMessage (NetworkMessage netMsg) {
            QSB.LogToScreen("Messager Receive");
            SectorMessage msg = netMsg.ReadMessage<SectorMessage>();

            if (isServer) {
                if (msg.id == connectionToClient.connectionId) {
                    NetworkServer.SendToAll(MsgType.Highest + 1, msg);
                    SetSectorById(msg.sector);
                }
            } else {
                if (msg.id == connectionToServer.connectionId) {
                    SetSectorById(msg.sector);
                }
            }
        }

        void SetSectorById (int sectorId) {
            QSB.LogToScreen("Set sector by ID");
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
            if (!isLocalPlayer) {
                QSB.LogToScreen("EnterSector being called for non-local player! Bad!");
            }
            var name = sector.GetName();
            if (name != Sector.Name.Unnamed) {
                QSB.LogToScreen("Client entered sector", name.ToString());
                SectorMessage msg = new SectorMessage();
                msg.sector = (int) sector.GetName();
                msg.id = connectionToServer.connectionId;
                if (isServer) {
                    NetworkServer.SendToAll(MsgType.Highest + 1, msg);
                } else {
                    connectionToServer.Send(MsgType.Highest + 1, msg);
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
