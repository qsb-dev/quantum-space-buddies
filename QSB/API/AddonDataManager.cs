using System;
using System.Collections.Generic;
using QSB.Utility;

namespace QSB.API;

public static class AddonDataManager
{
	private static readonly Dictionary<string, (Type objectType, Action<object> action)> _handlerDict = new();

	public static void OnReceiveDataMessage(string messageType, object data)
	{
		DebugLog.DebugWrite($"Received data message of message type \"{messageType}\"!");
		if (!_handlerDict.TryGetValue(messageType, out var handler))
		{
			return;
		}

		handler.action(Convert.ChangeType(data, handler.objectType));
	}

	public static void RegisterHandler<T>(string messageType, Action<T> handler)
	{
		DebugLog.DebugWrite($"Registering handler for \"{messageType}\" with type of {typeof(T).Name}");
		_handlerDict.Add(messageType, (typeof(T), o => handler((T)o)));
	}
}
