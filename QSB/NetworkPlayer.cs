using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class NetworkPlayer: NetworkBehaviour {
        Transform _body;
        bool _isSectorSetUp = false;
        public static NetworkPlayer localInstance { get; private set; }

        void Awake () {
            DontDestroyOnLoad(this);
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        void OnWakeUp () {
            DebugLog.Screen("Start NetworkPlayer", netId.Value);
            Invoke("SetFirstSector", 1);

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

        void SetFirstSector () {
            _isSectorSetUp = true;
            SectorSync.SetSector(netId.Value, Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform);
        }

        public void EnterSector (Sector sector) {
            SectorSync.SetSector(netId.Value, sector.GetName());
        }

        void Update () {
            if (!_body || !_isSectorSetUp) {
                return;
            }

            var sectorTransform = SectorSync.GetSector(netId.Value);

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
