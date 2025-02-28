using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

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
					{
						// mirror handles max connections. client will just get generic disconnect message, but its okay.
						var result = SteamNetworkingSockets.AcceptConnection(t.m_hConn);
						if (result != EResult.k_EResultOK)
						{
							_transport.Log($"[warn] accept {t.m_info.m_szConnectionDescription} returned {result}");
						}
						break;
					}
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					_conns.Add(t.m_hConn);
					_transport.OnServerConnected?.Invoke((int)t.m_hConn.m_HSteamNetConnection);
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
				// this logs an error below even tho it isnt really an error. its fine
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
					{
						var result = SteamNetworkingSockets.CloseConnection(t.m_hConn, t.m_info.m_eEndReason, t.m_info.m_szEndDebug, false);
						if (result != true)
						{
							_transport.Log($"[warn] close {t.m_info.m_szConnectionDescription} returned {result}");
						}
						_conns.Remove(t.m_hConn);
						_transport.OnServerError?.Invoke((int)t.m_hConn.m_HSteamNetConnection, TransportError.ConnectionClosed, t.m_info.m_szEndDebug);
						_transport.OnServerDisconnected?.Invoke((int)t.m_hConn.m_HSteamNetConnection);
						break;
					}
			}
		});
	}

	public bool IsListening;
	private HSteamListenSocket _listenSocket;
	// mirror connection id is derived from uint to int cast here. seems to do unchecked cast and be fine
	private readonly List<HSteamNetConnection> _conns = new();

	public void StartListening()
	{
		var options = Util.MakeOptions(_transport);

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
			_listenSocket = SteamNetworkingSockets.CreateListenSocketIP(ref steamAddr, options.Length, options);
			_transport.Log($"listening on {steamAddr.ToDebugString()}");
		}
		else
		{
			_listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, options.Length, options);
			_transport.Log($"listening on p2p");
		}
		IsListening = true;
	}

	public void Send(int connectionId, ArraySegment<byte> segment, int channelId)
	{
		var conn = new HSteamNetConnection((uint)connectionId);

		var result = conn.Send(segment, channelId);
		if (result != EResult.k_EResultOK)
		{
			_transport.Log($"[warn] send {conn.ToDebugString()} returned {result}");
		}
		_transport.OnServerDataSent?.Invoke(connectionId, segment, channelId);
	}

	public void Receive()
	{
		var ppOutMessages = new IntPtr[Util.MaxMessages];

		// receive can result in disconnect, which modifies the collection. we must copy
		foreach (var conn in _conns.ToList())
		{
			var numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(conn, ppOutMessages, ppOutMessages.Length);
			for (var i = 0; i < numMessages; i++)
			{
				var (segment, channelId) = Util.Receive(ppOutMessages[i]);
				_transport.OnServerDataReceived?.Invoke((int)conn.m_HSteamNetConnection, segment, channelId);
			}
		}
	}

	public void Flush()
	{
		foreach (var conn in _conns)
		{
			var result = SteamNetworkingSockets.FlushMessagesOnConnection(conn);
			if (result != EResult.k_EResultOK)
			{
				_transport.Log($"[warn] flush {conn.ToDebugString()} returned {result}");
			}
		}
	}

	public void Disconnect(int connectionId)
	{
		var conn = new HSteamNetConnection((uint)connectionId);
		_transport.Log($"disconnect {conn.ToDebugString()}");
		var result = SteamNetworkingSockets.CloseConnection(conn, 0, "disconnected by server", false);
		if (result != true)
		{
			_transport.Log($"[warn] close {conn.ToDebugString()} returned {result}");
		}
		_conns.Remove(conn);
		// its not an error for us to disconnect a client intentionally
		_transport.OnServerDisconnected?.Invoke(connectionId);
	}

	public void Close()
	{
		// mirror disconnects all clients for us before this
		_transport.Log("stop server");
		var result = SteamNetworkingSockets.CloseListenSocket(_listenSocket);
		if (result != true)
		{
			_transport.Log($"[warn] stop server returned {result}");
		}
		IsListening = false;

		_onStatusChanged.Dispose();
	}
}
