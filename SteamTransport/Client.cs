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
					// mirror will shutdown transport
					break;
			}
		});
	}

	public bool IsConnecting;
	public bool IsConnected;

	private HSteamNetConnection _conn;


	public void Connect(string address)
	{
		address = "127.0.0.1:1234";
		var steamAddr = new SteamNetworkingIPAddr();
		var parsed = steamAddr.ParseString(address);
		if (!parsed)
		{
			_transport.OnClientError(TransportError.DnsResolve, $"couldnt parse address {address} when connect");
			// should we call disconnect here? idk
			return;
		}

		_transport.Log($"connecting to {address}");
		_conn = SteamNetworkingSockets.ConnectByIPAddress(ref steamAddr, 0, new SteamNetworkingConfigValue_t[0]);
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
			var msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ppOutMessage); // cant pointer cast for some reason
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
		if (result != EResult.k_EResultOK)
			_transport.OnClientError?.Invoke(TransportError.Unexpected, $"flush returned {result}");
	}

	public void Close()
	{
		_transport.Log("client close");
		SteamNetworkingSockets.CloseConnection(_conn, 0, "client closed connection", false);
		IsConnecting = false;
		IsConnected = false;
		// should this do error?
		_transport.OnClientDisconnected?.Invoke();
		// mirror will shutdown transport

		_onStatusChanged.Dispose();
	}
}
