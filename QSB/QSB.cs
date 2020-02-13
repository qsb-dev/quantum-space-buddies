using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB {
    public class QSB: ModBehaviour {
        public static IModHelper Helper;
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
            Helper = ModHelper;

            gameObject.AddComponent<QSBNetworkManager>();
            gameObject.AddComponent<NetworkManagerHUD>();
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
    }
}
