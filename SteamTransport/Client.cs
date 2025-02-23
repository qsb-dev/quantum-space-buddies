using Mirror;
using Steamworks;
using System;
using System.Runtime.InteropServices;
using IDisposable = Delaunay.Utils.IDisposable;

namespace SteamTransport;

public class Client : IDisposable
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
					break;
			}
		});
	}

	public bool IsConnecting;
	public bool IsConnected;

	private HSteamNetConnection _conn;


	public void Connect(string address)
	{
		var ipAddr = new SteamNetworkingIPAddr();
		var parsed = ipAddr.ParseString(address);
		if (!parsed)
		{
			_transport.OnClientError(TransportError.DnsResolve, $"couldnt parse address {address} when connect");
			return;
		}

		_transport.Log($"connecting to {address}");
		_conn = SteamNetworkingSockets.ConnectByIPAddress(ref ipAddr, 0, new SteamNetworkingConfigValue_t[0]);
	}

	public void Dispose()
	{
		_onStatusChanged.Dispose();
	}

	public void RecieveData()
	{
		var ppOutMessages = new IntPtr[10];
		SteamNetworkingSockets.ReceiveMessagesOnConnection(_conn, ppOutMessages, 10);

		foreach (var ppOutMessage in ppOutMessages)
		{
			var msg = Marshal.PtrToStructure<SteamNetworkingMessage_t>(ppOutMessage);
			// do joe
			msg.Release();
		}
	}

	public void Flush()
	{
		SteamNetworkingSockets.FlushMessagesOnConnection(_conn);
	}

	public void Send(ArraySegment<byte> segment, int channelId)
	{
		throw new NotImplementedException();
	}
}
