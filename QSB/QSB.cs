using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class QSB: ModBehaviour {
        static QSB _instance;
        public static Dictionary<uint, NetworkPlayer> players;

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

            players = new Dictionary<uint, NetworkPlayer>();

            var assetBundle = ModHelper.Assets.LoadBundle("assets/network");
            var networkManager = Instantiate(assetBundle.LoadAsset<GameObject>("assets/networkmanager.prefab"));
            var networkPlayerPrefab = assetBundle.LoadAsset<GameObject>("assets/networkplayer.prefab");
            networkPlayerPrefab.AddComponent<NetworkPlayer>();
            networkManager.GetComponent<NetworkManager>().playerPrefab = networkPlayerPrefab;

            ModHelper.HarmonyHelper.AddPrefix<PlayerSectorDetector>("OnAddSector", typeof(Patches), "OnAddSector");
        }

        static string JoinAll (params object[] logObjects) {
            var result = "";
            foreach (var obj in logObjects) {
                result += obj + " ";
            }
            return result;
        }

        //public static void Log (params object[] logObjects) {
        //    _instance.ModHelper.Console.WriteLine(JoinAll(logObjects));
        //}

        public static void LogToScreen (params object[] logObjects) {
            NotificationData data = new NotificationData(NotificationTarget.Player, JoinAll(logObjects), 5f, true);
            NotificationManager.SharedInstance.PostNotification(data, false);
        }

        public static void OnReceiveMessage (NetworkMessage netMsg) {
            QSB.LogToScreen("global message receive");
            SectorMessage msg = netMsg.ReadMessage<SectorMessage>();
            players[msg.senderId].OnReceiveMessage(msg.sectorId);
        }

        static class Patches {
            static void OnAddSector (Sector sector, PlayerSectorDetector __instance) {
                if (NetworkPlayer.localInstance != null) {
                    NetworkPlayer.localInstance.EnterSector(sector);
                }
            }
        }
    }
}
