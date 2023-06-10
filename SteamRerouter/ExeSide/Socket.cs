using Steamworks;
using System.Net;
using System.Net.Sockets;
using static SteamRerouter.ExeSide.Program;

namespace SteamRerouter.ExeSide;

/// <summary>
/// handles communication with the mod
/// </summary>
public static class Socket
{
	private static TcpClient _tcpClient;

	public static void Connect(int port)
	{
		Log($"connecting to {port}");
		_tcpClient = new TcpClient();
		_tcpClient.Connect(IPAddress.Loopback, port);
	}

	/// <summary>
	/// runs a message loop until it receives a quit message
	///
	/// the mod sends us messages and then we respond and wait for the next message
	/// </summary>
	public static void Loop()
	{
		Log("message loop");

		while (true)
		{
			// recv message

			SteamAPI.RunCallbacks();
		}
	}
}
