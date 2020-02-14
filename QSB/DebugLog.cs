using UnityEngine;
using UnityEngine.UI;

namespace QSB {
    class DebugLog: MonoBehaviour {
        static Text _screenText;
        void Awake () {
            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/debug");
            var LogCanvas = Instantiate(assetBundle.LoadAsset<GameObject>("assets/logcanvas.prefab"));
            _screenText = LogCanvas.GetComponentInChildren<Text>();
        }

        static string JoinAll (params object[] logObjects) {
            var result = "";
            foreach (var obj in logObjects) {
                result += obj + " ";
            }
            return result;
        }

        public static void Console (params object[] logObjects) {
            QSB.Helper.Console.WriteLine(JoinAll(logObjects));
        }

        public static void Screen (params object[] logObjects) {
            _screenText.text = JoinAll(logObjects);
        }

        public static void HUD (params object[] logObjects) {
            if (Locator.GetPlayerBody() == null) {
                Console("Warning: tried to log to HUD but player is not ready.");
                Console(logObjects);
                return;
            }
            NotificationData data = new NotificationData(NotificationTarget.Player, JoinAll(logObjects), 5f, true);
            NotificationManager.SharedInstance.PostNotification(data, false);
        }
    }
}
