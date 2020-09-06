using OWML.Common;

namespace QSB.Utility
{
    public static class DebugLog
    {
        public static void ToConsole(string message, MessageType type = MessageType.Message)
        {
            QSB.Helper.Console.WriteLine(message, type);
        }

        public static void ToHud(string message)
        {
            if (Locator.GetPlayerBody() == null)
            {
                return;
            }
            var data = new NotificationData(NotificationTarget.Player, message.ToUpper());
            NotificationManager.SharedInstance.PostNotification(data);
        }

        public static void ToAll(string message, MessageType type = MessageType.Message)
        {
            ToConsole(message, type);
            ToHud(message);
        }

        public static void DebugWrite(string message, MessageType type = MessageType.Message)
        {
            if (QSB.DebugMode)
            {
                ToConsole(message, type);
            }
        }

        public static void LogState(string name, bool state)
        {
            var status = state ? "OK" : "FAIL";
            var messageType = state ? MessageType.Success : MessageType.Error;
            DebugWrite($"* {name} {status}", messageType);
        }

    }
}
