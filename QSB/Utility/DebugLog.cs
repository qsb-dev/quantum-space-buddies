using OWML.Common;
using OWML.Logging;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

#pragma warning disable CS0618

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

		if (QSBCore.Helper == null)
		{
			// yes i know this is only meant for OWML, but it's useful as a backup
			ModConsole.OwmlConsole.WriteLine(message, type, GetCallingType());
		}
		else
		{
			QSBCore.Helper.Console.WriteLine(message, type, GetCallingType());
		}
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
		if (QSBCore.DebugSettings.DebugMode)
		{
			ToConsole(message, type);
		}
	}

	private static string GetCallingType() =>
		new StackTrace(2) // skip this function and calling function
			.GetFrames()!
			.Select(x => x.GetMethod().DeclaringType!)
			.First(x => x != typeof(DebugLog) && !x.IsDefined(typeof(CompilerGeneratedAttribute), true))
			.Name;
}
