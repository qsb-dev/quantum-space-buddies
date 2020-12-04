using System.Collections.Generic;
using UnityEngine;

namespace QSB.QuantumUNET
{
	internal class QSBLocalClient : QSBNetworkClient
	{
		public override void Disconnect()
		{
			QSBClientScene.HandleClientDisconnect(m_Connection);
			if (m_Connected)
			{
				PostInternalMessage(33);
				m_Connected = false;
			}
			m_AsyncConnect = QSBNetworkClient.ConnectState.Disconnected;
			m_LocalServer.RemoveLocalClient(m_Connection);
		}

		internal void InternalConnectLocalServer(bool generateConnectMsg)
		{
			if (m_FreeMessages == null)
			{
				m_FreeMessages = new Stack<InternalMsg>();
				for (var i = 0; i < 64; i++)
				{
					var t = default(InternalMsg);
					m_FreeMessages.Push(t);
				}
			}
			m_LocalServer = QSBNetworkServer.instance;
			m_Connection = new QSBULocalConnectionToServer(m_LocalServer);
			base.SetHandlers(m_Connection);
			m_Connection.connectionId = m_LocalServer.AddLocalClient(this);
			m_AsyncConnect = QSBNetworkClient.ConnectState.Connected;
			QSBNetworkClient.SetActive(true);
			base.RegisterSystemHandlers(true);
			if (generateConnectMsg)
			{
				PostInternalMessage(32);
			}
			m_Connected = true;
		}

		internal override void Update() => ProcessInternalMessages();

		internal void AddLocalPlayer(QSBPlayerController localPlayer)
		{
			Debug.Log(string.Concat(new object[]
			{
				"Local client AddLocalPlayer ",
				localPlayer.Gameobject.name,
				" conn=",
				m_Connection.connectionId
			}));
			m_Connection.isReady = true;
			m_Connection.SetPlayerController(localPlayer);
			var unetView = localPlayer.UnetView;
			if (unetView != null)
			{
				QSBClientScene.SetLocalObject(unetView.NetId, localPlayer.Gameobject);
				unetView.SetConnectionToServer(m_Connection);
			}
			QSBClientScene.InternalAddPlayer(unetView, localPlayer.PlayerControllerId);
		}

		private void PostInternalMessage(byte[] buffer, int channelId)
		{
			InternalMsg item;
			if (m_FreeMessages.Count == 0)
			{
				item = default;
			}
			else
			{
				item = m_FreeMessages.Pop();
			}
			item.buffer = buffer;
			item.channelId = channelId;
			m_InternalMsgs.Add(item);
		}

		private void PostInternalMessage(short msgType)
		{
			var networkWriter = new QSBNetworkWriter();
			networkWriter.StartMessage(msgType);
			networkWriter.FinishMessage();
			PostInternalMessage(networkWriter.AsArray(), 0);
		}

		private void ProcessInternalMessages()
		{
			if (m_InternalMsgs.Count != 0)
			{
				var internalMsgs = m_InternalMsgs;
				m_InternalMsgs = m_InternalMsgs2;
				for (var i = 0; i < internalMsgs.Count; i++)
				{
					var t = internalMsgs[i];
					if (s_InternalMessage.Reader == null)
					{
						s_InternalMessage.Reader = new QSBNetworkReader(t.buffer);
					}
					else
					{
						s_InternalMessage.Reader.GetType().GetMethod("Replace", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(s_InternalMessage.Reader, new object[] { t.buffer });
					}
					s_InternalMessage.Reader.ReadInt16();
					s_InternalMessage.ChannelId = t.channelId;
					s_InternalMessage.Connection = base.connection;
					s_InternalMessage.MsgType = s_InternalMessage.Reader.ReadInt16();
					m_Connection.InvokeHandler(s_InternalMessage);
					m_FreeMessages.Push(t);
					base.connection.lastMessageTime = Time.time;
				}
				m_InternalMsgs = internalMsgs;
				m_InternalMsgs.Clear();
				for (var j = 0; j < m_InternalMsgs2.Count; j++)
				{
					m_InternalMsgs.Add(m_InternalMsgs2[j]);
				}
				m_InternalMsgs2.Clear();
			}
		}

		internal void InvokeHandlerOnClient(short msgType, QSBMessageBase msg, int channelId)
		{
			var networkWriter = new QSBNetworkWriter();
			networkWriter.StartMessage(msgType);
			msg.Serialize(networkWriter);
			networkWriter.FinishMessage();
			InvokeBytesOnClient(networkWriter.AsArray(), channelId);
		}

		internal void InvokeBytesOnClient(byte[] buffer, int channelId) => PostInternalMessage(buffer, channelId);

		private const int k_InitialFreeMessagePoolSize = 64;

		private List<InternalMsg> m_InternalMsgs = new List<InternalMsg>();

		private readonly List<InternalMsg> m_InternalMsgs2 = new List<InternalMsg>();

		private Stack<InternalMsg> m_FreeMessages;

		private QSBNetworkServer m_LocalServer;

		private bool m_Connected;

		private readonly QSBNetworkMessage s_InternalMessage = new QSBNetworkMessage();

		private struct InternalMsg
		{
			internal byte[] buffer;

			internal int channelId;
		}
	}
}