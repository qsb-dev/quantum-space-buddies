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

			// requesting connection, automatically allow
			if (t.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
			{
				SteamNetworkingSockets.AcceptConnection(t.m_hConn);
			}
		});

		var address = new SteamNetworkingIPAddr();
		address.ParseString("0.0.0.0:1234");
		address.ToString(out var addressStr, true);
		var socket = SteamNetworkingSockets.CreateListenSocketIP(ref address, 0, new SteamNetworkingConfigValue_t[0]);
		Console.WriteLine($"listening on {addressStr}...");

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
		address.ParseString("127.0.0.1:1234");
		address.ToString(out var addressStr, true);
		Console.WriteLine($"is localhost = {address.IsLocalHost()}");
		var conn = SteamNetworkingSockets.ConnectByIPAddress(ref address, 1, new SteamNetworkingConfigValue_t[]
		{
			new SteamNetworkingConfigValue_t
			{
				m_eValue = ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth,
				m_eDataType = ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
				m_val = new SteamNetworkingConfigValue_t.OptionValue
				{
					m_int32 = 1,
				}
			}
		});
		Console.WriteLine($"connecting to {addressStr}...");

		while (true)
		{
			SteamAPI.RunCallbacks();
			Thread.Sleep(10);
		}
	}
}
