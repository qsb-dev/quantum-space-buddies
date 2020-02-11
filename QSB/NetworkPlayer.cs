using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class NetworkPlayer: NetworkBehaviour {
        Transform _body;
        float _smoothSpeed = 10f;
        public static NetworkPlayer localInstance { get; private set; }

        [SyncVar(hook = "OnChangeSector")]
        public Sector.Name _sector;
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
        }

        public void EnterSector (Sector sector) {
            if (!isLocalPlayer) {
                QSB.LogToScreen("EnterSector being called for non-local player! Bad!");
            }
            var name = sector.GetName();
            if (name != Sector.Name.Unnamed) {
                QSB.LogToScreen("Client entered sector", name.ToString());
                CmdSetSector(name);
            }
        }

        [Command]
        void CmdSetSector (Sector.Name name) {
            if (!isServer) {
                QSB.LogToScreen("This is not the server, so skipping CmdSetSector");
                return;
            }
            QSB.LogToScreen("This is iserver, so setting client sector to", name.ToString());
            _sector = name;
        }

        void OnChangeSector (Sector.Name name) {
            QSB.LogToScreen("Client received onChange from server, to sector", name.ToString());
            _sector = name;

            var sectors = GameObject.FindObjectsOfType<Sector>();
            foreach (var sector in sectors) {
                if (name == sector.GetName()) {
                    _sectorTransform = sector.transform;
                    return;
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
