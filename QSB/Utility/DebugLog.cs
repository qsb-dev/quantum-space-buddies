using OWML.Common;
using OWML.Logging;
using OWML.Utils;
using System.Diagnostics;
using System.Linq;
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

		// copied from https://github.com/ow-mods/owml/blob/master/src/OWML.Logging/ModSocketOutput.cs#L33
		{
			var Logger = ModConsole.OwmlConsole.GetValue<IModLogger>("Logger");
			var _socket = ModConsole.OwmlConsole.GetValue<IModSocket>("_socket");

			Logger?.Log($"{type}: {message}");

			_socket.WriteToSocket(new ModSocketMessage
			{
				SenderName = "QSB",
				SenderType = GetCallingType(),
				Type = type,
				Message = message
			});

			if (type == MessageType.Fatal)
			{
				_socket.Close();
				Process.GetCurrentProcess().Kill();
			}
		}
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
			// BUG: this part doesnt work for some reason
			.First(x => x != typeof(DebugLog) && !x.IsDefined(typeof(CompilerGeneratedAttribute), true))
			.Name;
}
