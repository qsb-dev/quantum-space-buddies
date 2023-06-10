using System.Net;
using System.Net.Sockets;

namespace SteamRerouter.ModSide;

/// <summary>
/// handles communication with the exe
/// </summary>
public static class Socket
{
	private static TcpListener _listener;
	private static TcpClient _tcpClient;

	public static int Listen()
	{
		_listener = new TcpListener(IPAddress.Loopback, 0);
		_listener.Start();
		var port = ((IPEndPoint)_listener.LocalEndpoint).Port;
		Interop.Log($"port is {port}");
		return port;
	}

	public static void Accept()
	{
		Interop.Log("accepting");
		_tcpClient = _listener.AcceptTcpClient();
	}

	public static void Quit()
	{
		_listener.Stop();
		_tcpClient.Close();
	}
}
