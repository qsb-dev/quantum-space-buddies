using Steamworks;
using System;

namespace SteamTransport;

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
			// stupid copied init code
			{
				if (!Packsize.Test())
				{
					Console.Error.WriteLine("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
				}

				if (!DllCheck.Test())
				{
					Console.Error.WriteLine("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
				}

				// qsbcore
				Environment.SetEnvironmentVariable("SteamAppId", "480");
				Environment.SetEnvironmentVariable("SteamGameId", "480");

				var m_bInitialized = SteamAPI.Init();
				if (!m_bInitialized)
				{
					Console.Error.WriteLine("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");

					return;
				}
			}

			switch (Console.ReadKey().KeyChar)
			{
				case '1':
					DoServer();
					break;
				case '2':
					DoClient();
					break;
			}

		}
		finally
		{
			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}
	}

	private static void DoServer()
	{
		var address = new SteamNetworkingIPAddr();
		address.SetIPv6LocalHost();
		var socket = SteamNetworkingSockets.CreateListenSocketIP(ref address, 0, new SteamNetworkingConfigValue_t[0]);
	}

	private static void DoClient()
	{
		var address = new SteamNetworkingIPAddr();
		address.SetIPv6LocalHost();
		var conn = SteamNetworkingSockets.ConnectByIPAddress(ref address, 0, new SteamNetworkingConfigValue_t[0]);


	}
}
