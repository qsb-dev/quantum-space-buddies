using OWML.Common;
using OWML.Logging;
using QSB.WorldSync;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace QSB.Utility;

public static class DebugLog
{
	public static readonly int ProcessInstanceId = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName)
		.IndexOf(x => x.Id == Process.GetCurrentProcess().Id);

	public static void ToConsole(string message, MessageType type = MessageType.Message)
	{
		if (QSBCore.DebugSettings.InstanceIdInLogs)
		{
			message = $"[{ProcessInstanceId}] " + message;
		}

		QSBCore.Helper.Console.WriteLine(message, type, GetCallingType(new StackTrace()));
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
		if (QSBCore.Helper == null)
		{
			// yes i know this is only meant for OWML, but it's useful as a backup
			ModConsole.OwmlConsole.WriteLine(message, type, GetCallingType(new StackTrace()));
			return;
		}

		if (QSBCore.DebugSettings.DebugMode)
		{
			ToConsole(message, type);
		}
	}

	private static string GetCallingType(StackTrace frame) =>
		frame.GetFrames()!
			.Select(x => x.GetMethod().DeclaringType!.Name)
			.First(x => x != nameof(DebugLog));
}
