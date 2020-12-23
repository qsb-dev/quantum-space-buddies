using UnityEngine;

namespace QuantumUNET.Logging
{
	public static class QLog
	{
		private static QLogType LogType = QLogType.Warning | QLogType.Error | QLogType.FatalError;

		public static void SetLogType(QLogType flags)
			=> LogType = flags;

		public static void SetSpecifcLogType(QLogType flag, bool state)
		{
			if (state)
			{
				FlagsHelper.Set(ref LogType, flag);
				return;
			}
			FlagsHelper.Unset(ref LogType, flag);
		}

		public static void LogDebug(string message)
		{
			if (!FlagsHelper.IsSet(LogType, QLogType.Debug))
			{
				return;
			}
			Debug.Log($"DEBUG : {message}");
		}

		public static void Log(string message)
		{
			if (!FlagsHelper.IsSet(LogType, QLogType.Log))
			{
				return;
			}
			Debug.Log($"LOG : {message}");
		}

		public static void LogWarning(string message)
		{
			if (!FlagsHelper.IsSet(LogType, QLogType.Warning))
			{
				return;
			}
			Debug.LogWarning($"WARN : {message}");
		}

		public static void LogError(string message)
		{
			if (!FlagsHelper.IsSet(LogType, QLogType.Error))
			{
				return;
			}
			Debug.LogError($"ERROR : {message}");
		}

		public static void LogFatalError(string message)
		{
			if (!FlagsHelper.IsSet(LogType, QLogType.FatalError))
			{
				return;
			}
			Debug.LogError($"FATAL : {message}");
		}
	}
}
