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

            ModHelper.Events.Subscribe<PlayerBody>(Events.AfterStart);
            ModHelper.Events.OnEvent += OnEvent;

            var assetBundle = ModHelper.Assets.LoadBundle("assets/network");
            var networkManager = Instantiate(assetBundle.LoadAsset<GameObject>("assets/networkmanager.prefab"));
            var networkPlayerPrefab = assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            networkPlayerPrefab.AddComponent<NetworkPlayer>();
            networkManager.GetComponent<NetworkManager>().playerPrefab = networkPlayerPrefab;
        }

        void OnEvent (MonoBehaviour behaviour, Events ev) {
            var player = GameObject.Find("Traveller_HEA_Player_v2");

            var spawn1 = Locator.GetPlayerBody().gameObject;
            spawn1.AddComponent<NetworkStartPosition>();
            var spawn2 = Locator.GetShipBody().gameObject;
            spawn2.AddComponent<NetworkStartPosition>();

            var networkIdentity = player.AddComponent<NetworkIdentity>();
            networkIdentity.localPlayerAuthority = true;

            var networkTransform = player.AddComponent<NetworkTransform>();

            var networkManager = gameObject.AddComponent<NetworkManager>();
            networkManager.playerPrefab = player;
            var networkHUD = gameObject.AddComponent<NetworkManagerHUD>();
        }

        public static void Log (params string[] strings) {
            _instance.ModHelper.Console.WriteLine(string.Join(" ", strings));
        }
    }
}
