using Steamworks;
using System;
using IDisposable = Delaunay.Utils.IDisposable;

namespace SteamTransport;

public class Server : IDisposable
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
					pInfo.m_addrRemote.ToString(out var addr, true);
					_transport.Log($"accepting conn from {addr}");

					// ignore max connections for now
					SteamNetworkingSockets.AcceptConnection(t.m_hConn);
					break;
			}
		});
	}

	public bool Listening { get; set; }

	public void Dispose()
	{
		_onStatusChanged.Dispose();
	}

	public void RecieveData()
	{
		throw new NotImplementedException();
	}

	public void Flush()
	{
		throw new NotImplementedException();
	}

	public void Send(int connectionId, ArraySegment<byte> segment, int channelId) { }

	public void Disconnect(int connectionId)
	{
		throw new NotImplementedException();
	}

	public void StartListening()
	{
		throw new NotImplementedException();
	}
}
