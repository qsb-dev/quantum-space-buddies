using OWML.Common;
using System;
using System.Collections.Generic;

namespace QSB.Utility
{
	public static class GlobalMessenger<T, U, V, W, X, Y>
	{
		public static void AddListener(string eventType, Callback<T, U, V, W, X, Y> handler)
		{
			object obj = _eventTable;
			lock (obj)
			{
				if (!_eventTable.TryGetValue(eventType, out var eventData))
				{
					eventData = new EventData();
					_eventTable.Add(eventType, eventData);
				}

				eventData.Callbacks.Add(handler);
			}
		}

		public static void RemoveListener(string eventType, Callback<T, U, V, W, X, Y> handler)
		{
			object obj = _eventTable;
			lock (obj)
			{
				if (_eventTable.TryGetValue(eventType, out var eventData))
				{
					var num = eventData.Callbacks.IndexOf(handler);
					if (num >= 0)
					{
						eventData.Callbacks[num] = eventData.Callbacks[eventData.Callbacks.Count - 1];
						eventData.Callbacks.RemoveAt(eventData.Callbacks.Count - 1);
					}
				}
			}
		}

		public static void FireEvent(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6)
		{
			object obj = _eventTable;
			lock (obj)
			{
				if (_eventTable.TryGetValue(eventType, out var eventData))
				{
					if (eventData.IsInvoking)
					{
						throw new InvalidOperationException("GlobalMessenger does not support recursive FireEvent calls to the same eventType.");
					}

					eventData.IsInvoking = true;
					eventData.Temp.AddRange(eventData.Callbacks);
					for (var i = 0; i < eventData.Temp.Count; i++)
					{
						try
						{
							eventData.Temp[i](arg1, arg2, arg3, arg4, arg5, arg6);
						}
						catch (Exception exception)
						{
							DebugLog.ToConsole($"Error - {exception.Message}", MessageType.Error);
						}
					}

					eventData.Temp.Clear();
					eventData.IsInvoking = false;
				}
			}
		}

		private static readonly IDictionary<string, EventData> _eventTable = new Dictionary<string, EventData>(ComparerLibrary.stringEqComparer);

		private class EventData
		{
			public List<Callback<T, U, V, W, X, Y>> Callbacks = new();
			public List<Callback<T, U, V, W, X, Y>> Temp = new();
			public bool IsInvoking;
		}
	}
}
