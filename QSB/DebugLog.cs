using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace QSB
{
    public class DebugLog : MonoBehaviour
    {
        private const int ScreenLinesMax = 6;

        private static Text _screenText;
        private static List<string> _lines;
        private static GameObject _logCanvas;

        private void Awake()
        {
            var assetBundle = QSB.Helper.Assets.LoadBundle("assets/debug");
            _logCanvas = Instantiate(assetBundle.LoadAsset<GameObject>("assets/logcanvas.prefab"));
            DontDestroyOnLoad(_logCanvas);
            DontDestroyOnLoad(this);
            _logCanvas.GetComponent<Canvas>().sortingOrder = 9999;
            _screenText = _logCanvas.GetComponentInChildren<Text>();

            _lines = new List<string>(ScreenLinesMax);
            for (var i = 0; i < ScreenLinesMax; i++)
            {
                _lines.Add(".");
            }
        }

        public static void UpdateInfoCanvas(uint netId, InfoState state, object value)
        {
            var stateName = Enum.GetName(typeof(InfoState), state);
            var stateGameObject = _logCanvas.transform.Find("InfoCanvases").Find(netId.ToString()).Find(stateName);
            stateGameObject.transform.Find("ValueText").GetComponent<Text>().text = value.ToString();
        }

        private static string JoinAll(params object[] logObjects)
        {
            return string.Join(" ", logObjects.Select(o => o.ToString()).ToArray());
        }

        public static void ToConsole(string message)
        {
            QSB.Helper.Console.WriteLine(message, MessageType.Message);
        }

        public static void ToConsole(string message, MessageType type)
        {
            QSB.Helper.Console.WriteLine(message, type);
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
            var data = new NotificationData(NotificationTarget.Player, JoinAll(logObjects));
            NotificationManager.SharedInstance.PostNotification(data);
        }

        public static void ToAll(params object[] logObjects)
        {
            ToConsole(JoinAll(logObjects));
            ToScreen(logObjects);
            ToHud(logObjects);
        }
    }

    public enum InfoState
    {
        ID,
        Name,
        Sector,
        RelativePosition,
        Suit,
        Flashlight,
        Signalscope
    }
}
