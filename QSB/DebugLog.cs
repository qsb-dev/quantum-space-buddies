using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QSB {
    class DebugLog: MonoBehaviour {
        static Text _screenText;
        static List<string> _lines;
        static readonly int _screenLinesMax = 6;

        void Awake () {
            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/debug");
            var LogCanvas = Instantiate(assetBundle.LoadAsset<GameObject>("assets/logcanvas.prefab"));
            LogCanvas.GetComponent<Canvas>().sortingOrder = 9999;
            _screenText = LogCanvas.GetComponentInChildren<Text>();

            _lines = new List<string>(_screenLinesMax);
            for (var i = 0; i < _screenLinesMax; i++) {
                _lines.Add(".");
            }
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
            for (var i = 1; i < _screenLinesMax; i++) {
                _lines[i - 1] = _lines[i];
            }
            _lines.Insert(_screenLinesMax - 1, JoinAll(logObjects));
            _screenText.text = String.Join("\n", _lines.ToArray());
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
