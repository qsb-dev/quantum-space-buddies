using System;
using System.Collections.Generic;
using QSB.Utility;

namespace QSB.API;

public static class AddonDataManager
{
	private static readonly Dictionary<string, Action<uint, object>> _handlers = new();

	public static void OnReceiveDataMessage(string messageType, object data, uint from)
	{
		DebugLog.DebugWrite($"Received data message of message type \"{messageType}\" from {from}!");
		if (!_handlers.TryGetValue(messageType, out var handler))
		{
			return;
		}

		handler(from, data);
	}

	public static void RegisterHandler<T>(string messageType, Action<uint, T> handler)
	{
		DebugLog.DebugWrite($"Registering handler for \"{messageType}\" with type of {typeof(T).Name}");
		_handlers.Add(messageType, (from, data) => handler(from, (T)data));
	}
}
