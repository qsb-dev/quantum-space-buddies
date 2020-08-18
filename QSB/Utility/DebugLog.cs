using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using OWML.Common;
using UnityEngine;

namespace QSB.Utility
{
    public class DebugLog : MonoBehaviour
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

        public static string GenerateTable(List<string> collumsData, List<string> rowData)
        {
            var longestKey = collumsData.OrderByDescending(s => s.Length).First();
            var longestValue = rowData.OrderByDescending(s => s.Length).First();
            var longestObject = (longestKey.Length > longestValue.Length) ? longestKey : longestValue;
            string collums = "|";
            string data = "|";
            foreach (var item in collumsData)
            {
                collums += " " + item.PadRight(longestObject.Length) + " |";
                
            }
            foreach (var item in rowData)
            {
                data += " " + item.PadRight(longestObject.Length) + " |";
            }
            return collums + Environment.NewLine + data;
        }
    }
}
