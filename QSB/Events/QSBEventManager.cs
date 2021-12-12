using System;
using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using QSB.Utility;

namespace QSB.Events
{
	public static class QSBEventManager
	{
		public static bool Ready { get; private set; }

		private static readonly Type[] _types = typeof(IQSBEvent).GetDerivedTypes().ToArray();
		internal static readonly List<IQSBEvent> _eventList = new();

		public static void Init()
		{
			foreach (var type in _types)
			{
				_eventList.Add((IQSBEvent)Activator.CreateInstance(type));
			}

			if (UnitTestDetector.IsInUnitTest)
			{
				return;
			}

			_eventList.ForEach(ev => ev.SetupListener());

			Ready = true;

			DebugLog.DebugWrite("Event Manager ready.", MessageType.Success);
		}

		public static void Reset()
		{
			Ready = false;
			_eventList.ForEach(ev => ev.CloseListener());
			_eventList.Clear();
		}

		public static void FireEvent(string eventName)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				return;
			}

			GlobalMessenger.FireEvent(eventName);
		}

		public static void FireEvent<T>(string eventName, T arg)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T>.FireEvent(eventName, arg);
		}

		public static void FireEvent<T, U>(string eventName, T arg1, U arg2)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U>.FireEvent(eventName, arg1, arg2);
		}

		public static void FireEvent<T, U, V>(string eventName, T arg1, U arg2, V arg3)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U, V>.FireEvent(eventName, arg1, arg2, arg3);
		}

		public static void FireEvent<T, U, V, W>(string eventName, T arg1, U arg2, V arg3, W arg4)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U, V, W>.FireEvent(eventName, arg1, arg2, arg3, arg4);
		}

		public static void FireEvent<T, U, V, W, X>(string eventName, T arg1, U arg2, V arg3, W arg4, X arg5)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U, V, W, X>.FireEvent(eventName, arg1, arg2, arg3, arg4, arg5);
		}

		public static void FireEvent<T, U, V, W, X, Y>(string eventName, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U, V, W, X, Y>.FireEvent(eventName, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		/// used to force set ForId for every sent event
		public static uint ForIdOverride = uint.MaxValue;
	}
}
