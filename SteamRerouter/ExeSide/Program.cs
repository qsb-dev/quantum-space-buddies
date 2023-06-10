using HarmonyLib;
using Steamworks;
using System;
using System.IO;
using System.Reflection;

namespace SteamRerouter.ExeSide;

/// <summary>
/// top level file on the exe
/// </summary>
public static class Program
{
	private static int Main(string[] args)
	{
		var managedDir = args[0];
		Log($"managed dir = {managedDir}");

		AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
		{
			var name = new AssemblyName(e.Name).Name + ".dll";
			var path = Path.Combine(managedDir, name);
			return File.Exists(path) ? Assembly.LoadFile(path) : null;
		};

		var type = int.Parse(args[1]);
		Log($"command type = {type}");
		var arg = int.Parse(args[2]);
		Log($"command arg = {arg}");

		return DoCommand(type, arg);
	}

	public static void Log(object msg) => Console.Out.WriteLine(msg);
	public static void LogError(object msg) => Console.Error.WriteLine(msg);

	private static int DoCommand(int type, int arg = default)
	{
		// copied from QSBCore
		if (!Packsize.Test())
		{
			LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
		}

		if (!DllCheck.Test())
		{
			LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
		}

		// from facepunch.steamworks SteamClient.cs
		Environment.SetEnvironmentVariable("SteamAppId", "753640");
		Environment.SetEnvironmentVariable("SteamGameId", "753640");

		if (!SteamAPI.Init())
		{
			LogError($"FATAL - SteamAPI.Init() failed. Refer to Valve's documentation.");
			return -1;
		}

		var exitCode = -1;
		switch (type)
		{
			// dlc status
			case 0:
				var owned = SteamApps.BIsDlcInstalled((AppId_t)1622100U);
				Log($"dlc owned: {owned}");
				exitCode = owned ? 1 : 0;
				break;

			// earn achievement
			case 1:
				var achievementType = (Achievements.Type)arg;
				Log("Earn " + achievementType);
				// for some reason even with unsafe code turned on it throws a FieldAccessException
				var s_names = (string[])AccessTools.Field(typeof(Achievements), "s_names").GetValue(null);
				if (!SteamUserStats.SetAchievement(s_names[(int)achievementType]))
				{
					LogError("Unable to grant achievement \"" + s_names[(int)achievementType] + "\"");
				}
				else
				{
					exitCode = 0;
				}
				SteamUserStats.StoreStats();
				break;
		}

		SteamAPI.Shutdown();
		return exitCode;
	}
}
