﻿using QuantumUNET.Messages;
using QuantumUNET.Transport;
using System.Collections.Generic;
using UnityEngine;

namespace QuantumUNET
{
	internal class QLocalClient : QNetworkClient
	{
		public override void Disconnect()
		{
			QClientScene.HandleClientDisconnect(m_Connection);
			if (m_Connected)
			{
				PostInternalMessage(33);
				m_Connected = false;
			}

			m_AsyncConnect = ConnectState.Disconnected;
			m_LocalServer.RemoveLocalClient();
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

			m_LocalServer = QNetworkServer.instance;
			m_Connection = new QULocalConnectionToServer(m_LocalServer);
			SetHandlers(m_Connection);
			m_Connection.connectionId = m_LocalServer.AddLocalClient(this);
			m_AsyncConnect = ConnectState.Connected;
			SetActive(true);
			RegisterSystemHandlers(true);
			if (generateConnectMsg)
			{
				PostInternalMessage(32);
			}

			m_Connected = true;
		}

		internal override void Update() => ProcessInternalMessages();

		internal void AddLocalPlayer(QPlayerController localPlayer)
		{
			Debug.Log($"Local client AddLocalPlayer {localPlayer.Gameobject.name} conn={m_Connection.connectionId}");
			m_Connection.isReady = true;
			m_Connection.SetPlayerController(localPlayer);
			var unetView = localPlayer.UnetView;
			if (unetView != null)
			{
				QClientScene.SetLocalObject(unetView.NetId, localPlayer.Gameobject);
				unetView.SetConnectionToServer(m_Connection);
			}

			QClientScene.InternalAddPlayer(unetView, localPlayer.PlayerControllerId);
		}

		private void PostInternalMessage(byte[] buffer, int channelId)
		{
			var item = m_FreeMessages.Count == 0 ? default : m_FreeMessages.Pop();
			item.buffer = buffer;
			item.channelId = channelId;
			m_InternalMsgs.Add(item);
		}

		private void PostInternalMessage(short msgType)
		{
			var networkWriter = new QNetworkWriter();
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
				foreach (var msg in internalMsgs)
				{
					if (s_InternalMessage.Reader == null)
					{
						s_InternalMessage.Reader = new QNetworkReader(msg.buffer);
					}
					else
					{
						s_InternalMessage.Reader.Replace(msg.buffer);
					}

					s_InternalMessage.Reader.ReadInt16();
					s_InternalMessage.ChannelId = msg.channelId;
					s_InternalMessage.Connection = connection;
					s_InternalMessage.MsgType = s_InternalMessage.Reader.ReadInt16();
					m_Connection.InvokeHandler(s_InternalMessage);
					m_FreeMessages.Push(msg);
					connection.lastMessageTime = Time.time;
				}

				m_InternalMsgs = internalMsgs;
				m_InternalMsgs.Clear();
				foreach (var msg in m_InternalMsgs2)
				{
					m_InternalMsgs.Add(msg);
				}

				m_InternalMsgs2.Clear();
			}
		}

		internal void InvokeHandlerOnClient(short msgType, QMessageBase msg, int channelId)
		{
			var networkWriter = new QNetworkWriter();
			networkWriter.StartMessage(msgType);
			msg.Serialize(networkWriter);
			networkWriter.FinishMessage();
			InvokeBytesOnClient(networkWriter.AsArray(), channelId);
		}

		internal void InvokeBytesOnClient(byte[] buffer, int channelId) => PostInternalMessage(buffer, channelId);

		private const int k_InitialFreeMessagePoolSize = 64;

		private List<InternalMsg> m_InternalMsgs = new();

		private readonly List<InternalMsg> m_InternalMsgs2 = new();

		private Stack<InternalMsg> m_FreeMessages;

		private QNetworkServer m_LocalServer;

		private bool m_Connected;

		private readonly QNetworkMessage s_InternalMessage = new();

		private struct InternalMsg
		{
			internal byte[] buffer;

			internal int channelId;
		}
	}
}