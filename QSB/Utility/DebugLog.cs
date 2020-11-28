using OWML.Common;
using OWML.Logging;
using System.Diagnostics;
using System.Linq;

namespace QSB.Utility
{
	public static class DebugLog
	{
		public static void ToConsole(string message, MessageType type = MessageType.Message)
		{
			// make custom method name in owml log.
			// i wrote the owml code for this so this is fine?? shut up i dont want to change owml
			var console = (ModSocketOutput)QSB.Helper.Console;
			var method = console.GetType()
				.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.Last(x => x.Name == "WriteLine");
			var callingType = GetCallingType(new StackTrace());
			method.Invoke(console, new object[] { type, message, callingType });
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

		private static string GetCallingType(StackTrace frame)
		{
			var stackFrame = frame.GetFrames().First(x => x.GetMethod().DeclaringType.Name != "DebugLog");
			return stackFrame.GetMethod().DeclaringType.Name;
		}
	}
}
