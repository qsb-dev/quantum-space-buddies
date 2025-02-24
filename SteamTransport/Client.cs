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
				$"state = {t.m_info.m_eState}\n" +
				$"end = {(ESteamNetConnectionEnd)t.m_info.m_eEndReason} {t.m_info.m_szEndDebug}\n");

			switch (t.m_info.m_eState)
			{
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
					IsConnecting = true;
					IsConnected = false;
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					_transport.OnClientConnected?.Invoke();
					IsConnecting = false;
					IsConnected = true;
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
					_transport.OnClientError?.Invoke(TransportError.ConnectionClosed, t.m_info.m_szEndDebug);
					SteamNetworkingSockets.CloseConnection(_conn, t.m_info.m_eEndReason, t.m_info.m_szEndDebug, false);
					_transport.OnClientDisconnected?.Invoke();
					IsConnecting = false;
					IsConnected = false;
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
		var data = new byte[segment.Count];
		Array.Copy(segment.Array, segment.Offset, data, 0, data.Length);
		unsafe
		{
			fixed (byte* pData = data)
			{
				var result = SteamNetworkingSockets.SendMessageToConnection(_conn, (IntPtr)pData, (uint)data.Length, Util.MirrorChannel2SendFlag(channelId), out _);
				_transport.OnClientDataSent?.Invoke(segment, channelId);
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
			unsafe
			{
				var msg = *(SteamNetworkingMessage_t*)ppOutMessage; // probably not gonna work
				var data = new byte[msg.m_cbSize];
				Marshal.Copy(msg.m_pData, data, 0, data.Length);
				var channel = Util.SendFlag2MirrorChannel(msg.m_nFlags);
				_transport.OnClientDataReceived?.Invoke(new ArraySegment<byte>(data), channel);
				msg.Release();
			}
		}
	}

	public void Flush()
	{
		var result = SteamNetworkingSockets.FlushMessagesOnConnection(_conn);
	}

	public void Close()
	{
		_transport.Log("client close");
		SteamNetworkingSockets.CloseConnection(_conn, 0, "client closed connection", false);
		_transport.OnClientDisconnected?.Invoke();

		_onStatusChanged.Dispose();
	}
}
