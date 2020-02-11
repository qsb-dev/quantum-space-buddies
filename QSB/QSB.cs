using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class QSB: ModBehaviour {
        static QSB _instance;

        void Awake () {
            Application.runInBackground = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update () {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Start () {
            _instance = this;

            var assetBundle = ModHelper.Assets.LoadBundle("assets/network");
            var networkManager = Instantiate(assetBundle.LoadAsset<GameObject>("assets/networkmanager.prefab"));
            var networkPlayerPrefab = assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            networkPlayerPrefab.AddComponent<NetworkPlayer>();
            networkManager.GetComponent<NetworkManager>().playerPrefab = networkPlayerPrefab;
        }

        public static void Log (params string[] strings) {
            _instance.ModHelper.Console.WriteLine(string.Join(" ", strings));
        }
    }
}
