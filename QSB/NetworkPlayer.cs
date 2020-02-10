using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class NetworkPlayer: NetworkBehaviour {
        Transform _body;
        Transform _sun;

        void Start () {
            QSB.Log("Started network player");

            _sun = Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform;

            var player = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");
            if (isLocalPlayer) {
                _body = player;
            } else {
                _body = Instantiate(player);
                _body.parent = transform;
                _body.localPosition = Vector3.zero;
                _body.localRotation = Quaternion.identity;
            }
        }
        void Update () {
            if (!_body) {
                return;
            }
            if (isLocalPlayer) {
                transform.position = _body.position - _sun.position;
                transform.rotation = _body.rotation * Quaternion.Inverse(_sun.rotation);
            } else {
                _body.position = _sun.position + transform.position;
                _body.rotation = transform.rotation * _sun.rotation;
            }
        }
    }
}
