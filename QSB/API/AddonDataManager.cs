using System;
using System.Collections.Generic;
using QSB.Utility;

namespace QSB.API;

public static class AddonDataManager
{
	private static readonly Dictionary<string, Action<object>> _handlers = new();

	public static void OnReceiveDataMessage(string messageType, object data)
	{
		DebugLog.DebugWrite($"Received data message of message type \"{messageType}\"!");
		if (!_handlers.TryGetValue(messageType, out var handler))
		{
			return;
		}

		handler(data);
	}

	public static void RegisterHandler<T>(string messageType, Action<T> handler)
	{
		DebugLog.DebugWrite($"Registering handler for \"{messageType}\" with type of {typeof(T).Name}");
		_handlers.Add(messageType, data => handler((T)data));
	}
}
