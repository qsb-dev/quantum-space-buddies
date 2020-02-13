using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class QSB: ModBehaviour {
        public static IModHelper Helper;
        static QSB _instance;
        public static Dictionary<uint, Transform> playerSectors;

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
            Helper = ModHelper;

            playerSectors = new Dictionary<uint, Transform>();

            gameObject.AddComponent<QSBNetworkManager>();
            gameObject.AddComponent<NetworkManagerHUD>();

            ModHelper.HarmonyHelper.AddPrefix<PlayerSectorDetector>("OnAddSector", typeof(Patches), "OnAddSector");
        }

        static string JoinAll (params object[] logObjects) {
            var result = "";
            foreach (var obj in logObjects) {
                result += obj + " ";
            }
            return result;
        }

        public static void Log (params object[] logObjects) {
            _instance.ModHelper.Console.WriteLine(JoinAll(logObjects));
        }

        public static void LogToScreen (params object[] logObjects) {
            if (Locator.GetPlayerBody() == null) {
                Log("Warning: tried to log to HUD but player is not ready.");
                Log(logObjects);
                return;
            }
            NotificationData data = new NotificationData(NotificationTarget.Player, JoinAll(logObjects), 5f, true);
            NotificationManager.SharedInstance.PostNotification(data, false);
        }

        public static Transform GetSectorByName (Sector.Name sectorName) {
            var sectors = GameObject.FindObjectsOfType<Sector>();
            foreach (var sector in sectors) {
                if (sectorName == sector.GetName()) {
                    return sector.transform;
                }
            }
            return null;
        }

        public static void OnReceiveMessage (NetworkMessage netMsg) {
            QSB.LogToScreen("Global message receive");
            SectorMessage msg = netMsg.ReadMessage<SectorMessage>();

            var sectorName = (Sector.Name) msg.sectorId;
            var sectorTransform = GetSectorByName(sectorName);

            if (sectorTransform == null) {
                QSB.LogToScreen("Sector", sectorName, "not found");
                return;
            }

            QSB.LogToScreen("Found sector", sectorName, ", setting for", msg.senderId);

            playerSectors[msg.senderId] = sectorTransform;
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
