using System;
using System.Collections.Generic;
using OWML.Common;
using QSB.Utility;

namespace QSB.API;

public static class AddonDataManager
{
	private static readonly Dictionary<int, Action<uint, object>> _handlers = new();

	public static void OnReceiveDataMessage(int hash, object data, uint from)
	{
		if (!_handlers.TryGetValue(hash, out var handler))
		{
			DebugLog.DebugWrite($"unknown addon message type with hash {hash}", MessageType.Error);
			return;
		}
		handler(from, data);
	}

	public static void RegisterHandler<T>(int hash, Action<uint, T> handler)
	{
		_handlers.Add(hash, (from, data) => handler(from, (T)data));
	}
}
