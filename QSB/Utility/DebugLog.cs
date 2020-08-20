using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var data = new NotificationData(NotificationTarget.Player, message);
            NotificationManager.SharedInstance.PostNotification(data);
        }

        public static void ToAll(string message, MessageType type = MessageType.Message)
        {
            ToConsole(message, type);
            ToHud(message);
        }

        public static string GenerateTable(List<string> columnsData, List<string> rowData)
        {
            var longestKey = columnsData.OrderByDescending(s => s.Length).First();
            var longestValue = rowData.OrderByDescending(s => s.Length).First();
            var longestObject = (longestKey.Length > longestValue.Length) ? longestKey : longestValue;
            var columns = "|";
            var data = "|";
            foreach (var item in columnsData)
            {
                columns += " " + item.PadRight(longestObject.Length) + " |";
            }
            foreach (var item in rowData)
            {
                data += " " + item.PadRight(longestObject.Length) + " |";
            }
            return columns + Environment.NewLine + data;
        }

        public static void OkayState(string name, bool state)
        {
            if (state)
            {
                OkayState(name, State.OK);
                return;
            }
            OkayState(name, State.FAIL);
        }

        public static void OkayState(string name, State state)
        {
            if (state == State.FAIL)
            {
                ToConsole($"* {name} FAIL", MessageType.Error);
                return;
            }
            ToConsole($"* {name} OK", MessageType.Success);
        }

        public enum State
        {
            OK,
            FAIL
        }
    }
}
