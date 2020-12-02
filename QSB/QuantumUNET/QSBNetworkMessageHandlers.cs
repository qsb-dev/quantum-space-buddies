using System.Collections.Generic;
using UnityEngine;

namespace QSB.QuantumUNET
{
	class QSBNetworkMessageHandlers
	{
		internal void RegisterHandlerSafe(short msgType, QSBNetworkMessageDelegate handler)
		{
			if (handler == null)
			{
				Debug.LogError("RegisterHandlerSafe id:" + msgType + " handler is null");
			}
			else
			{
				Debug.Log(string.Concat(new object[]
				{
					"RegisterHandlerSafe id:",
					msgType,
					" handler:",
					handler.GetMethodName()
				}));
				if (!this.m_MsgHandlers.ContainsKey(msgType))
				{
					this.m_MsgHandlers.Add(msgType, handler);
				}
			}
		}

		public void RegisterHandler(short msgType, QSBNetworkMessageDelegate handler)
		{
			if (handler == null)
			{
				Debug.LogError("RegisterHandler id:" + msgType + " handler is null");
			}
			else if (msgType <= 31)
			{
				Debug.LogError("RegisterHandler: Cannot replace system message handler " + msgType);
			}
			else
			{
				if (this.m_MsgHandlers.ContainsKey(msgType))
				{
					Debug.Log("RegisterHandler replacing " + msgType);
					this.m_MsgHandlers.Remove(msgType);
				}
				Debug.Log(string.Concat(new object[]
				{
					"RegisterHandler id:",
					msgType,
					" handler:",
					handler.GetMethodName()
				}));
				this.m_MsgHandlers.Add(msgType, handler);
			}
		}

		public void UnregisterHandler(short msgType)
		{
			this.m_MsgHandlers.Remove(msgType);
		}

		internal QSBNetworkMessageDelegate GetHandler(short msgType)
		{
			QSBNetworkMessageDelegate result;
			if (this.m_MsgHandlers.ContainsKey(msgType))
			{
				result = this.m_MsgHandlers[msgType];
			}
			else
			{
				result = null;
			}
			return result;
		}

		internal Dictionary<short, QSBNetworkMessageDelegate> GetHandlers()
		{
			return this.m_MsgHandlers;
		}

		internal void ClearMessageHandlers()
		{
			this.m_MsgHandlers.Clear();
		}

		private Dictionary<short, QSBNetworkMessageDelegate> m_MsgHandlers = new Dictionary<short, QSBNetworkMessageDelegate>();
	}
}
