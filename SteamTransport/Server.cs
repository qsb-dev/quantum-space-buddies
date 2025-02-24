using Steamworks;
using System;

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
			Console.WriteLine($"STATUS CHANGED for {t.m_info.m_szConnectionDescription}\n" +
				$"state = {t.m_info.m_eState}\n" +
				$"end = {(ESteamNetConnectionEnd)t.m_info.m_eEndReason} {t.m_info.m_szEndDebug}");

			switch (t.m_info.m_eState)
			{
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
					SteamNetworkingSockets.GetConnectionInfo(t.m_hConn, out var pInfo);
					pInfo.m_addrRemote.ToString(out var address, true);
					_transport.Log($"accepting conn from {address}");

					// ignore max connections for now
					SteamNetworkingSockets.AcceptConnection(t.m_hConn);
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
					break;
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
				case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
					break;
			}
		});
	}

	public bool IsListening;
	private HSteamListenSocket _listenSocket;

	public void StartListening()
	{
		var address = "0.0.0.0:1234";
		var steamAddr = new SteamNetworkingIPAddr();
		steamAddr.ParseString(address);
		_listenSocket = SteamNetworkingSockets.CreateListenSocketIP(ref steamAddr, 0, new SteamNetworkingConfigValue_t[0]);
		_transport.Log($"listening on {address}");
		IsListening = true;
	}

	public void Send(int connectionId, ArraySegment<byte> segment, int channelId) { }

	public void RecieveData() { }

	public void Flush() { }

	public void Disconnect(int connectionId) { }

	public void Close()
	{
		SteamNetworkingSockets.CloseListenSocket(_listenSocket);
		_transport.Log("stop server");

		_onStatusChanged.Dispose();
	}
}
