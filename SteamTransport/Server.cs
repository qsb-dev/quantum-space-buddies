using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SteamTransport;

// could check more Result stuff for functions. idc rn
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
					// mirror handles max connections. client will just get generic disconnect message, but its okay.
					SteamNetworkingSockets.AcceptConnection(t.m_hConn);
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					_conns.Add(t.m_hConn);
					_transport.OnServerConnected?.Invoke((int)t.m_hConn.m_HSteamNetConnection);
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
					// this logs an error below even tho it isnt really an error. its fine
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
	// mirror connection id is derived from uint to int cast here. seems to do unchecked cast and be fine
	private readonly List<HSteamNetConnection> _conns = new();

	public void StartListening()
	{
		if (!string.IsNullOrEmpty(_transport.TestIpAddress))
		{
			var steamAddr = new SteamNetworkingIPAddr();
			var parsed = steamAddr.ParseString(_transport.TestIpAddress);
			if (!parsed)
			{
				_transport.OnServerError?.Invoke(-1, TransportError.DnsResolve, $"couldnt parse address {_transport.TestIpAddress} when listening");
				// dont really need to stop server here. mirror isnt designed to let us fail to listen anyway so this is all kinda silly
				return;
			}
			_listenSocket = SteamNetworkingSockets.CreateListenSocketIP(ref steamAddr, 0, new SteamNetworkingConfigValue_t[0]);
			_transport.Log($"listening on {steamAddr.DebugToString()}");
		}
		else
		{
			_listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, new SteamNetworkingConfigValue_t[0]);
			_transport.Log($"listening on p2p");
		}
		IsListening = true;
	}

	public void Send(int connectionId, ArraySegment<byte> segment, int channelId)
	{
		var conn = new HSteamNetConnection((uint)connectionId);

		// from fizzy
		var data = new byte[segment.Count];
		Array.Copy(segment.Array, segment.Offset, data, 0, data.Length);
		var pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
		var pData = pinnedArray.AddrOfPinnedObject();

		var result = SteamNetworkingSockets.SendMessageToConnection(conn, pData, (uint)data.Length, Util.MirrorChannel2SendFlag(channelId), out _);
		if (result == EResult.k_EResultOK)
			_transport.OnServerDataSent?.Invoke(connectionId, segment, channelId);
		else
			_transport.OnServerError?.Invoke(connectionId, TransportError.InvalidSend, $"send returned {result}");
		// i dont think we have to check for disconnect result here since the status change handles that

		/*
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
				// i dont think we have to check for disconnect result here since the status change handles that
			}
		}
		*/
	}

	public void Receive()
	{
		var ppOutMessages = new IntPtr[Util.MaxMessages];

		foreach (var conn in _conns)
		{
			var numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(conn, ppOutMessages, ppOutMessages.Length);
			for (var i = 0; i < numMessages; i++)
			{
				var ppOutMessage = ppOutMessages[i];
				var msg = SteamNetworkingMessage_t.FromIntPtr(ppOutMessage);
				var data = new byte[msg.m_cbSize];
				Marshal.Copy(msg.m_pData, data, 0, msg.m_cbSize);
				var channel = Util.SendFlag2MirrorChannel(msg.m_nFlags);
				_transport.OnServerDataReceived?.Invoke((int)conn.m_HSteamNetConnection, new ArraySegment<byte>(data), channel);
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
			// i dont think we have to check for disconnect result here since the status change handles that
		}
	}

	public void Disconnect(int connectionId)
	{
		var conn = new HSteamNetConnection((uint)connectionId);
		_transport.Log($"disconnect {conn.DebugToString()}");
		SteamNetworkingSockets.CloseConnection(conn, 0, "disconnected by server", false);
		_conns.Remove(conn);
		// dont need an error for disconnecting client
		_transport.OnServerDisconnected?.Invoke(connectionId);
	}

	public void Close()
	{
		// mirror disconnects all clients for us before this
		_transport.Log("stop server");
		SteamNetworkingSockets.CloseListenSocket(_listenSocket);
		IsListening = false;

		_onStatusChanged.Dispose();
	}
}
