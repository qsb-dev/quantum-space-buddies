using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QSB
{
    public class DebugLog : MonoBehaviour
    {
        private const int ScreenLinesMax = 10;

        private static Text _screenText;
        private static List<string> _lines;

        private void Awake()
        {
            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/debug");
            var logCanvas = Instantiate(assetBundle.LoadAsset<GameObject>("assets/logcanvas.prefab"));
            DontDestroyOnLoad(logCanvas);
            DontDestroyOnLoad(this);
            logCanvas.GetComponent<Canvas>().sortingOrder = 9999;
            _screenText = logCanvas.GetComponentInChildren<Text>();

            _lines = new List<string>(ScreenLinesMax);
            for (var i = 0; i < ScreenLinesMax; i++)
            {
                _lines.Add(".");
            }
        }

        private static string JoinAll(params object[] logObjects)
        {
            return string.Join(" ", logObjects.Select(o => o.ToString()).ToArray());
        }

        public static void ToConsole(params object[] logObjects)
        {
            QSB.Helper.Console.WriteLine(logObjects);
        }

        public static void ToScreen(params object[] logObjects)
        {
            for (var i = 1; i < ScreenLinesMax; i++)
            {
                _lines[i - 1] = _lines[i];
            }
            _lines.Insert(ScreenLinesMax - 1, JoinAll(logObjects));
            _screenText.text = string.Join("\n", _lines.ToArray());
        }

        public static void ToHud(params object[] logObjects)
        {
            if (Locator.GetPlayerBody() == null)
            {
                //ToConsole("Warning: tried to log to HUD but player is not ready.");
                //ToConsole("* " + JoinAll(logObjects));
                return;
            }
            var data = new NotificationData(NotificationTarget.Player, JoinAll(logObjects), 5f, true);
            NotificationManager.SharedInstance.PostNotification(data, false);
        }

        public static void ToAll(params object[] logObjects)
        {
            ToConsole(logObjects);
            ToScreen(logObjects);
            ToHud(logObjects);
        }
    }
}
