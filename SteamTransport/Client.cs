using Mirror;
using Steamworks;
using System;
using System.Runtime.InteropServices;

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
					SteamNetworkingSockets.CloseConnection(_conn, t.m_info.m_eEndReason, t.m_info.m_szEndDebug, false);
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
		if (!string.IsNullOrEmpty(_transport.TestIpAddress))
		{
			var steamAddr = new SteamNetworkingIPAddr();
			var parsed = steamAddr.ParseString(_transport.TestIpAddress);
			if (!parsed)
			{
				_transport.OnClientError?.Invoke(TransportError.DnsResolve, $"couldnt parse address {address} when connecting");
				_transport.OnClientDisconnected?.Invoke(); // will show error box
				return;
			}

			_conn = SteamNetworkingSockets.ConnectByIPAddress(ref steamAddr, 0, new SteamNetworkingConfigValue_t[0]);
			_transport.Log($"connecting to {steamAddr.DebugToString()}");
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

			_conn = SteamNetworkingSockets.ConnectP2P(ref identity, 0, 0, new SteamNetworkingConfigValue_t[0]);
			_transport.Log($"connecting to {identity.DebugToString()}");
		}
	}

	public void Send(ArraySegment<byte> segment, int channelId)
	{
		// use pointer to managed array instead of making copy. is this okay?
		unsafe
		{
			fixed (byte* pData = segment.Array)
			{
				var result = SteamNetworkingSockets.SendMessageToConnection(_conn, (IntPtr)(pData + segment.Offset), (uint)segment.Count, Util.MirrorChannel2SendFlag(channelId), out _);
				if (result == EResult.k_EResultOK)
					_transport.OnClientDataSent?.Invoke(segment, channelId);
				else
					_transport.OnClientError?.Invoke(TransportError.InvalidSend, $"send returned {result}");
				// i dont think we have to check for disconnect result here since the status change handles that
			}
		}
	}

	public void Receive()
	{
		const int maxMessages = 10;
		var ppOutMessages = new IntPtr[maxMessages];
		var numMessages = SteamNetworkingSockets.ReceiveMessagesOnConnection(_conn, ppOutMessages, maxMessages);
		for (var i = 0; i < numMessages; i++)
		{
			var ppOutMessage = ppOutMessages[i];
			var msg = SteamNetworkingMessage_t.FromIntPtr(ppOutMessage);
			var data = new byte[msg.m_cbSize];
			Marshal.Copy(msg.m_pData, data, 0, data.Length);
			var channel = Util.SendFlag2MirrorChannel(msg.m_nFlags);
			_transport.OnClientDataReceived?.Invoke(new ArraySegment<byte>(data), channel);
			SteamNetworkingMessage_t.Release(ppOutMessage);
		}
	}

	public void Flush()
	{
		var result = SteamNetworkingSockets.FlushMessagesOnConnection(_conn);
		if (result != EResult.k_EResultOK && result != EResult.k_EResultIgnored) // k_EResultIgnored gives spam when connecting
			_transport.OnClientError?.Invoke(TransportError.Unexpected, $"flush returned {result}");
		// i dont think we have to check for disconnect result here since the status change handles that
	}

	public void Close()
	{
		_transport.Log("client close");
		SteamNetworkingSockets.CloseConnection(_conn, 0, "client closed connection", false);
		IsConnecting = false;
		IsConnected = false;
		// dont need to do error because we dont show dialogue box for intentional disconnect
		_transport.OnClientDisconnected?.Invoke();

		_onStatusChanged.Dispose();
	}
}
