using Steamworks;
using System;
using System.Threading;

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

				SteamClient.SetWarningMessageHook((severity, text) => Console.WriteLine(text));
			}

			switch (Console.ReadKey().KeyChar)
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

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}
	}

	private static void DoServer()
	{
		using var cb = Steamworks.Callback<SteamNetConnectionStatusChangedCallback_t>.Create(t =>
		{
			Console.WriteLine($"{t.m_info.m_szConnectionDescription} | state = {t.m_info.m_eState} | {(ESteamNetConnectionEnd)t.m_info.m_eEndReason} {t.m_info.m_szEndDebug}");
		});

		var address = new SteamNetworkingIPAddr();
		address.Clear();
		var socket = SteamNetworkingSockets.CreateListenSocketIP(ref address, 0, new SteamNetworkingConfigValue_t[0]);
		Console.WriteLine("listening...");

		while (true)
		{
			SteamAPI.RunCallbacks();
			Thread.Sleep(10);
		}
	}

	private static void DoClient()
	{
		using var cb = Steamworks.Callback<SteamNetConnectionStatusChangedCallback_t>.Create(t =>
		{
			Console.WriteLine($"{t.m_info.m_szConnectionDescription} | state = {t.m_info.m_eState} | {(ESteamNetConnectionEnd)t.m_info.m_eEndReason} {t.m_info.m_szEndDebug}");
		});

		var address = new SteamNetworkingIPAddr();
		address.SetIPv6LocalHost();
		Console.WriteLine($"is localhost = {address.IsLocalHost()}");
		var conn = SteamNetworkingSockets.ConnectByIPAddress(ref address, 0, new SteamNetworkingConfigValue_t[0]);
		Console.WriteLine("connecting...");

		while (true)
		{
			SteamAPI.RunCallbacks();
			Thread.Sleep(10);
		}
	}
}
