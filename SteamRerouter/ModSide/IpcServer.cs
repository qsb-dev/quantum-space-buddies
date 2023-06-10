using System.Net;
using System.Net.Sockets;
using static SteamRerouter.ModSide.Interop;

namespace SteamRerouter.ModSide;

/// <summary>
/// handles communication with the exe
/// </summary>
public static class IpcServer
{
	private static TcpListener _listener;
	private static TcpClient _tcpClient;

	public static int Listen()
	{
		_listener = new TcpListener(IPAddress.Loopback, 0);
		_listener.Start();
		var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
		Log($"port is {port}");
		return port;
	}

	public static void Accept()
	{
		Log("accepting");
		_tcpClient = _listener.AcceptTcpClient();
	}

	public static void Quit()
	{
		Log("quitting");
		_listener.Stop();
		_tcpClient.Close();
	}

	public static EntitlementsManager.AsyncOwnershipStatus SteamEntitlementRetriever_GetOwnershipStatus()
	{
		Log("get ownership status");
		return EntitlementsManager.AsyncOwnershipStatus.Owned;
	}

	public static void Achievements_Earn(Achievements.Type type)
	{
		Log($"earn achievement {type}");
	}
}
