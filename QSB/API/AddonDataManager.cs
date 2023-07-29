using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QSB.Utility;
using UnityEngine;

namespace QSB.API;

public static class AddonDataManager
{
	private static Dictionary<string, (Type objectType, dynamic action)> _handlerDict = new();

	public static void OnReceiveDataMessage(string messageType, object data)
	{
		DebugLog.DebugWrite($"Received data message of message type \"{messageType}\"!");
		if (!_handlerDict.ContainsKey(messageType))
		{
			return;
		}

		_handlerDict[messageType].action(Convert.ChangeType(data, _handlerDict[messageType].objectType));
	}

	public static void RegisterHandler<T>(string messageType, Action<T> handler)
	{
		DebugLog.DebugWrite($"Registering handler for \"{messageType}\" with type of {typeof(T).Name}");
		_handlerDict.Add(messageType, (typeof(T), handler));
	}
}
