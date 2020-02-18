using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class QSB: ModBehaviour {
        public static IModHelper Helper;

        private void Awake () {
            Application.runInBackground = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update () {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Start () {
            Helper = ModHelper;

            gameObject.AddComponent<DebugLog>();
            gameObject.AddComponent<QSBNetworkManager>();
            gameObject.AddComponent<NetworkManagerHUD>();
            gameObject.AddComponent<PreserveTimeScale>();
        }

    }
}
