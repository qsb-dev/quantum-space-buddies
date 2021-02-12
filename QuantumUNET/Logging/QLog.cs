namespace QuantumUNET.Logging
{
	public static class QLog
	{
		public const int DebugType = 0;
		public const int LogType = 1;
		public const int WarningType = 2;
		public const int ErrorType = 3;
		public const int FatalErrorType = 4;

		private static int _currentLog;
		private static bool _logDebug => _currentLog <= 0;
		private static bool _logLog => _currentLog <= 1;
		private static bool _logWarning => _currentLog <= 2;
		private static bool _logError => _currentLog <= 3;
		private static bool _logFatal => _currentLog <= 4;

		public static void SetLogType(int level)
			=> _currentLog = level;

		public static void Debug(string message)
		{
			if (_logDebug)
			{
				return;
			}
			UnityEngine.Debug.Log($"DEBUG : {message}");
		}

		public static void Log(string message)
		{
			if (_logLog)
			{
				return;
			}
			UnityEngine.Debug.Log($"LOG : {message}");
		}

		public static void Warning(string message)
		{
			if (_logWarning)
			{
				return;
			}
			UnityEngine.Debug.LogWarning($"WARN : {message}");
		}

		public static void Error(string message)
		{
			if (_logError)
			{
				return;
			}
			UnityEngine.Debug.LogError($"ERROR : {message}");
		}

		public static void FatalError(string message)
		{
			if (_logFatal)
			{
				return;
			}
			UnityEngine.Debug.LogError($"FATAL : {message}");
		}
	}
}
