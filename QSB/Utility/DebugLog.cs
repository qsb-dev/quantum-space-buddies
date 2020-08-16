using System.Linq;
using OWML.Common;
using UnityEngine;

namespace QSB.Utility
{
    public class DebugLog : MonoBehaviour
    {
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

        public static void ToHud(params object[] logObjects)
        {
            if (Locator.GetPlayerBody() == null)
            {
                return;
            }
            var data = new NotificationData(NotificationTarget.Player, JoinAll(logObjects));
            NotificationManager.SharedInstance.PostNotification(data);
        }

        public static void ToAll(MessageType type, params object[] logObjects)
        {
            ToConsole(JoinAll(logObjects), type);
            ToHud(logObjects);
        }

        public static void ToAll(params object[] logObjects)
        {
            ToAll(MessageType.Message, logObjects);
        }
    }
}
