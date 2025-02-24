﻿using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SteamTransport;

public class Server
{
	private SteamTransport _transport;
	private Steamworks.Callback<SteamNetConnectionStatusChangedCallback_t> _onStatusChanged;

	public Server(SteamTransport transport)
	{
		_transport = transport;

		_onStatusChanged = Steamworks.Callback<SteamNetConnectionStatusChangedCallback_t>.Create(t =>
		{
			_transport.Log($"STATUS CHANGED for {t.m_info.m_szConnectionDescription}\n" +
				$"  state = {t.m_info.m_eState}\n" +
				$"  end = {(ESteamNetConnectionEnd)t.m_info.m_eEndReason} {t.m_info.m_szEndDebug}");
			// SteamNetworkingSockets.GetDetailedConnectionStatus(t.m_hConn, out var status, 1000);
			// _transport.Log(status);

			switch (t.m_info.m_eState)
			{
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
					// ignore max connections for now
					SteamNetworkingSockets.AcceptConnection(t.m_hConn);
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					_conns.Add(t.m_hConn);
					_transport.OnServerConnected?.Invoke((int)t.m_hConn.m_HSteamNetConnection);
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
					_transport.OnServerError?.Invoke((int)t.m_hConn.m_HSteamNetConnection, TransportError.ConnectionClosed, t.m_info.m_szEndDebug);
					SteamNetworkingSockets.CloseConnection(t.m_hConn, t.m_info.m_eEndReason, t.m_info.m_szEndDebug, false);
					_transport.OnServerDisconnected?.Invoke((int)t.m_hConn.m_HSteamNetConnection);
					_conns.Remove(t.m_hConn);
					break;
			}
		});
	}

	public bool IsListening;
	private HSteamListenSocket _listenSocket;
	private readonly List<HSteamNetConnection> _conns = new();

	public void StartListening()
	{
		var address = "0.0.0.0:1234";
		var steamAddr = new SteamNetworkingIPAddr();
		steamAddr.ParseString(address);
		_listenSocket = SteamNetworkingSockets.CreateListenSocketIP(ref steamAddr, 0, new SteamNetworkingConfigValue_t[0]);
		_transport.Log($"listening on {address}");
		IsListening = true;
	}

	public void Send(int connectionId, ArraySegment<byte> segment, int channelId)
	{
		var conn = new HSteamNetConnection((uint)connectionId);

		var data = new byte[segment.Count];
		Array.Copy(segment.Array, segment.Offset, data, 0, data.Length);
		unsafe
		{
			fixed (byte* pData = data)
			{
				var result = SteamNetworkingSockets.SendMessageToConnection(conn, (IntPtr)pData, (uint)data.Length, Util.MirrorChannel2SendFlag(channelId), out _);
				_transport.OnServerDataSent?.Invoke(connectionId, segment, channelId);
			}
		}
	}

	public void Receive()
	{
		const int maxMessages = 10;
		var ppOutMessages = new IntPtr[maxMessages];

		foreach (var conn in _conns)
		{
			var numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(conn, ppOutMessages, maxMessages);
			for (var i = 0; i < numMessages; i++)
			{
				var ppOutMessage = ppOutMessages[i];
				unsafe
				{
					var msg = *(SteamNetworkingMessage_t*)ppOutMessage; // probably not gonna work
					var data = new byte[msg.m_cbSize];
					Marshal.Copy(msg.m_pData, data, 0, data.Length);
					var channel = Util.SendFlag2MirrorChannel(msg.m_nFlags);
					_transport.OnServerDataReceived((int)conn.m_HSteamNetConnection, new ArraySegment<byte>(data), channel);
					msg.Release();
				}
			}
		}
	}

	public void Flush()
	{
		foreach (var conn in _conns)
		{
			var result = SteamNetworkingSockets.FlushMessagesOnConnection(conn);
		}
	}

	public void Disconnect(int connectionId)
	{
		var conn = new HSteamNetConnection((uint)connectionId);
		SteamNetworkingSockets.CloseConnection(conn, 0, "disconnected by server", false);
		_conns.Remove(conn);
	}

	public void Close()
	{
		_transport.Log("stop server");
		SteamNetworkingSockets.CloseListenSocket(_listenSocket);
		IsListening = false;

		_onStatusChanged.Dispose();
	}
}
