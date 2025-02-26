using HarmonyLib;
using Steamworks;
using System;
using System.Threading;

namespace SteamTransportTest;

/// <summary>
/// entry point for testing.
/// should probably make this a separate project since it copies over steam api. idc rn
/// </summary>
public static class Program
{
	public static void Main(string[] args)
	{
		Console.WriteLine("This is the test mode for the steam transport");

		try
		{
			// copied from qsbcore and steamworks.net docs
			{
				if (!Packsize.Test())
				{
					Console.Error.WriteLine("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
				}

				if (!DllCheck.Test())
				{
					Console.Error.WriteLine("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
				}

				// yes, these have to be the same. even for connecting to ip address
				Environment.SetEnvironmentVariable("SteamAppId", "480");
				Environment.SetEnvironmentVariable("SteamGameId", "480");

				var m_bInitialized = SteamAPI.Init();
				if (!m_bInitialized)
				{
					Console.Error.WriteLine("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");

					return;
				}

				SteamClient.SetWarningMessageHook((severity, text) => Console.WriteLine(text));
			}

			Console.WriteLine("press 1 for server, 2 for client");
			switch (Console.ReadKey(true).KeyChar)
			{
				case '1':
					Console.WriteLine("server");
					DoServer();
					break;
				case '2':
					Console.WriteLine("client");
					DoClient();
					break;
			}
		}
		finally
		{
			SteamAPI.Shutdown();

			Console.WriteLine("Done. Press any key to exit");
			Console.ReadKey();
		}
	}

	private static void DoServer()
	{
		var transport = new SteamTransport.SteamTransport();
		transport.Log = Console.WriteLine;
		transport.TestIpAddress = "127.0.0.1:1234";
		transport.Timeout = 1000;
		transport.DoFakeNetworkErrors = true;

		transport.OnServerError = (conn, error, s) => Console.Error.WriteLine($"ERROR {conn} {error} {s}");
		var theConn = -1;
		transport.OnServerConnected = conn => theConn = conn;
		transport.OnServerDataReceived = (conn, bytes, i) => Console.WriteLine($"RECV {conn} {bytes.Join()} {i}");

		transport.ServerStart();

		try
		{
			Console.WriteLine("press q to quit, s to send, d to disconnect");
			var running = true;
			while (running)
			{
				transport.ServerEarlyUpdate();
				if (Console.KeyAvailable)
				{
					switch (Console.ReadKey(true).KeyChar)
					{
						case 'q':
							running = false;
							break;
						case 's':
							transport.ServerSend(theConn, new ArraySegment<byte>(new byte[] { 1, 2, 3, 4, 5 }, 1, 5 - 1));
							break;
						case 'd':
							transport.ServerDisconnect(theConn);
							break;
					}
				}
				SteamAPI.RunCallbacks();
				transport.ServerLateUpdate();
				Thread.Sleep(10);
			}
		}
		finally
		{
			transport.ServerDisconnect(theConn); // mirror does this for us
			transport.ServerStop();
		}
	}

	private static void DoClient()
	{
		var transport = new SteamTransport.SteamTransport();
		transport.Log = Console.WriteLine;
		transport.TestIpAddress = "127.0.0.1:1234";
		transport.Timeout = 1000;
		transport.DoFakeNetworkErrors = true;

		transport.OnClientError = (error, s) => Console.Error.WriteLine($"ERROR {error} {s}");
		transport.OnClientDataReceived = (bytes, i) => Console.WriteLine($"RECV {bytes.Join()} {i}");

		transport.ClientConnect("76561198150564286");

		try
		{
			Console.WriteLine("press q to quit, s to send");

			var running = true;
			transport.OnClientDisconnected = () => running = false; // mirror normally does this
			while (running)
			{
				transport.ClientEarlyUpdate();
				if (Console.KeyAvailable)
				{
					switch (Console.ReadKey(true).KeyChar)
					{
						case 'q':
							running = false;
							break;
						case 's':
							transport.ClientSend(new ArraySegment<byte>(new byte[] { 1, 2, 3, 4, 5 }, 1, 5 - 1));
							break;
					}
				}
				SteamAPI.RunCallbacks();
				transport.ClientLateUpdate();
				Thread.Sleep(10);
			}
		}
		finally
		{
			transport.ClientDisconnect();
		}
	}
}
