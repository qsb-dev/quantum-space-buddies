using QuantumUNET.Logging;
using QuantumUNET.Messages;
using System.Collections.Generic;

namespace QuantumUNET
{
	internal class QNetworkMessageHandlers
	{
		private readonly Dictionary<short, QNetworkMessageDelegate> _msgHandlers = new Dictionary<short, QNetworkMessageDelegate>();

		internal void RegisterHandlerSafe(short msgType, QNetworkMessageDelegate handler)
		{
			if (handler == null)
			{
				QLog.Error($"RegisterHandlerSafe id:{msgType} handler is null");
			}
			else
			{
				QLog.Debug($"RegisterHandlerSafe id:{msgType} handler:{handler.GetMethodName()}");
				if (!_msgHandlers.ContainsKey(msgType))
				{
					_msgHandlers.Add(msgType, handler);
				}
			}
		}

		public void RegisterHandler(short msgType, QNetworkMessageDelegate handler)
		{
			if (handler == null)
			{
				QLog.Error($"RegisterHandler id:{msgType} handler is null");
			}
			else if (msgType <= 31)
			{
				QLog.Error($"RegisterHandler: Cannot replace system message handler {msgType}");
			}
			else
			{
				if (_msgHandlers.ContainsKey(msgType))
				{
					QLog.Log($"RegisterHandler replacing {msgType}");
					_msgHandlers.Remove(msgType);
				}

				QLog.Debug($"RegisterHandler id:{msgType} handler:{handler.GetMethodName()}");
				_msgHandlers.Add(msgType, handler);
			}
		}

		public void UnregisterHandler(short msgType) =>
			_msgHandlers.Remove(msgType);

		internal QNetworkMessageDelegate GetHandler(short msgType) =>
			_msgHandlers.ContainsKey(msgType) ? _msgHandlers[msgType] : null;

		internal Dictionary<short, QNetworkMessageDelegate> GetHandlers() => _msgHandlers;

		internal void ClearMessageHandlers() => _msgHandlers.Clear();
	}
}