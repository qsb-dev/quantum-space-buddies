﻿using HarmonyLib;
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
			Console.WriteLine($"STATUS CHANGED for {t.m_info.m_szConnectionDescription}\n" +
				$"state = {t.m_info.m_eState}\n" +
				$"end = {(ESteamNetConnectionEnd)t.m_info.m_eEndReason} {t.m_info.m_szEndDebug}");

			switch (t.m_info.m_eState)
			{
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
					IsConnecting = true;
					IsConnected = false;
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					IsConnecting = false;
					IsConnected = true;
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
					_transport.OnClientError(TransportError.ConnectionClosed, $"end = {(ESteamNetConnectionEnd)t.m_info.m_eEndReason} {t.m_info.m_szEndDebug}");
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
			return;
		}

		_transport.Log($"connecting to {address}");
		_conn = SteamNetworkingSockets.ConnectByIPAddress(ref steamAddr, 0, new SteamNetworkingConfigValue_t[0]);
	}

	public void Send(ArraySegment<byte> segment, int channelId)
	{
		throw new NotImplementedException();
	}

	public void RecieveData()
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
				var data = new ArraySegment<byte>(new Span<byte>((byte*)msg.m_pData, msg.m_cbSize).ToArray());
				var channel = Util.SendFlag2MirrorChannel(msg.m_nFlags);
				_transport.Log($"received data {data.Join()}");
				_transport.OnClientDataReceived(data, channel);
				msg.Release();
			}
		}
	}

	public void Flush()
	{
		SteamNetworkingSockets.FlushMessagesOnConnection(_conn);
	}

	public void Close()
	{
		// SteamNetworkingSockets.CloseConnection(_conn, )

		_onStatusChanged.Dispose();
	}
}
