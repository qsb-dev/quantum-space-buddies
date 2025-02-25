using Mirror;
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
					SteamNetworkingSockets.CloseConnection(t.m_hConn, t.m_info.m_eEndReason, t.m_info.m_szEndDebug, false);
					_conns.Remove(t.m_hConn);
					_transport.OnServerError?.Invoke((int)t.m_hConn.m_HSteamNetConnection, TransportError.ConnectionClosed, t.m_info.m_szEndDebug);
					_transport.OnServerDisconnected?.Invoke((int)t.m_hConn.m_HSteamNetConnection);
					break;
			}
		});
	}

	public bool IsListening;
	private HSteamListenSocket _listenSocket;
	// connection id is derived from uint to int cast here. is that okay???
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

		// use pointer to managed array instead of making copy. is this okay?
		unsafe
		{
			fixed (byte* pData = segment.Array)
			{
				var result = SteamNetworkingSockets.SendMessageToConnection(conn, (IntPtr)(pData + segment.Offset), (uint)segment.Count, Util.MirrorChannel2SendFlag(channelId), out _);
				if (result == EResult.k_EResultOK)
					_transport.OnServerDataSent?.Invoke(connectionId, segment, channelId);
				else
					_transport.OnServerError?.Invoke(connectionId, TransportError.InvalidSend, $"send returned {result}");
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
				var msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ppOutMessage); // cant pointer cast for some reason
				var data = new byte[msg.m_cbSize];
				Marshal.Copy(msg.m_pData, data, 0, data.Length);
				var channel = Util.SendFlag2MirrorChannel(msg.m_nFlags);
				_transport.OnServerDataReceived((int)conn.m_HSteamNetConnection, new ArraySegment<byte>(data), channel);
				SteamNetworkingMessage_t.Release(ppOutMessage);
			}
		}
	}

	public void Flush()
	{
		foreach (var conn in _conns)
		{
			var result = SteamNetworkingSockets.FlushMessagesOnConnection(conn);
			if (result != EResult.k_EResultOK)
				_transport.OnServerError?.Invoke((int)conn.m_HSteamNetConnection, TransportError.Unexpected, $"flush returned {result}");
		}
	}

	public void Disconnect(int connectionId)
	{
		var conn = new HSteamNetConnection((uint)connectionId);
		_transport.Log($"disconnect {conn.GetDescription()}");
		SteamNetworkingSockets.CloseConnection(conn, 0, "disconnected by server", false);
		_conns.Remove(conn);
		// should this do error?
		_transport.OnServerDisconnected?.Invoke((int)conn.m_HSteamNetConnection);
	}

	public void Close()
	{
		_transport.Log("stop server");
		// mirror disconnects all clients for us before this
		SteamNetworkingSockets.CloseListenSocket(_listenSocket);
		IsListening = false;

		_onStatusChanged.Dispose();
	}
}
