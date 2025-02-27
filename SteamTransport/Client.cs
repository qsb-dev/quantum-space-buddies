using Mirror;
using Steamworks;
using System;

namespace SteamTransport;

public class Client
{
	private SteamTransport _transport;
	private Steamworks.Callback<SteamNetConnectionStatusChangedCallback_t> _onStatusChanged;

	public Client(SteamTransport transport)
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
					IsConnecting = true;
					IsConnected = false;
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					IsConnecting = false;
					IsConnected = true;
					_transport.OnClientConnected?.Invoke();
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
					var result = SteamNetworkingSockets.CloseConnection(_conn, t.m_info.m_eEndReason, t.m_info.m_szEndDebug, false);
					if (result != true)
					{
						_transport.Log($"[warn] close returned {result}");
					}
					IsConnecting = false;
					IsConnected = false;
					_transport.OnClientError?.Invoke(TransportError.ConnectionClosed, t.m_info.m_szEndDebug);
					_transport.OnClientDisconnected?.Invoke();
					// OnClientDisconnected will cause mirror to shutdown the transport
					break;
			}
		});
	}

	public bool IsConnecting;
	public bool IsConnected;

	private HSteamNetConnection _conn;


	public void Connect(string address)
	{
		var options = Util.MakeOptions(_transport);

		if (!string.IsNullOrEmpty(_transport.TestIpAddress))
		{
			var steamAddr = new SteamNetworkingIPAddr();
			var parsed = steamAddr.ParseString(_transport.TestIpAddress);
			if (!parsed)
			{
				_transport.OnClientError?.Invoke(TransportError.DnsResolve, $"couldnt parse address {_transport.TestIpAddress} when connecting");
				_transport.OnClientDisconnected?.Invoke(); // will show error box
				return;
			}

			_conn = SteamNetworkingSockets.ConnectByIPAddress(ref steamAddr, options.Length, options);
			_transport.Log($"connecting to {steamAddr.ToDebugString()}");
		}
		else
		{
			var identity = new SteamNetworkingIdentity();
			var parsed = ulong.TryParse(address, out var steamId);
			if (!parsed)
			{
				_transport.OnClientError?.Invoke(TransportError.DnsResolve, $"couldnt parse address {address} when connecting");
				_transport.OnClientDisconnected?.Invoke(); // will show error box
				return;
			}
			identity.SetSteamID64(steamId);

			_conn = SteamNetworkingSockets.ConnectP2P(ref identity, 0, options.Length, options);
			_transport.Log($"connecting to {identity.ToDebugString()}");
		}
	}

	public void Send(ArraySegment<byte> segment, int channelId)
	{
		var result = _conn.Send(segment, channelId);
		if (result != EResult.k_EResultOK)
		{
			_transport.Log($"[warn] send returned {result}");
		}
		_transport.OnClientDataSent?.Invoke(segment, channelId);
	}

	public void Receive()
	{
		var ppOutMessages = new IntPtr[Util.MaxMessages];
		var numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(_conn, ppOutMessages, ppOutMessages.Length);
		for (var i = 0; i < numMessages; i++)
		{
			var (segment, channelId) = Util.Receive(ppOutMessages[i]);
			_transport.OnClientDataReceived?.Invoke(segment, channelId);
		}
	}

	public void Flush()
	{
		var result = SteamNetworkingSockets.FlushMessagesOnConnection(_conn);
		if (result != EResult.k_EResultOK && result != EResult.k_EResultIgnored) // flush does ignored when connecting. dont log cuz spam
		{
			_transport.Log($"[warn] flush returned {result}");
		}
	}

	public void Close()
	{
		// theres a weird case where we arent doing an intentional disconnect but there isnt a status change disonnect either
		// not sure whats going on there, but ill slap a stack trace on it

		_transport.Log($"client close\n{Environment.StackTrace}");
		var result = SteamNetworkingSockets.CloseConnection(_conn, 0, "client closed connection", false);
		if (result != true)
		{
			_transport.Log($"[warn] client close returned {result}");
		}
		IsConnecting = false;
		IsConnected = false;
		// its not an error for us to close ourselves intentionally
		// but we do it anyway cuz above comment
		_transport.OnClientError?.Invoke(TransportError.ConnectionClosed, "client closed connection, but you shouldnt be seeing this in game. make sure [DEBUG] Debug Mode is on and check logs for stack trace. report this please!");
		_transport.OnClientDisconnected?.Invoke();

		_onStatusChanged.Dispose();
	}
}
