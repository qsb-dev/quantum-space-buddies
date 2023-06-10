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
	private static void Main(string[] args)
	{
		var port = int.Parse(args[0]);
		Log($"port = {port}");
		var managedDir = args[1];
		Log($"managed dir = {managedDir}");

		AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
		{
			var name = new AssemblyName(e.Name).Name + ".dll";
			var path = Path.Combine(managedDir, name);
			return File.Exists(path) ? Assembly.LoadFile(path) : null;
		};

		Go(port);
	}

	private static void Go(int port)
	{
		Log("go");

		// copied from QSBCore
		if (!Packsize.Test())
		{
			Log("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
		}

		if (!DllCheck.Test())
		{
			Log("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
		}

		// from facepunch.steamworks SteamClient.cs
		Environment.SetEnvironmentVariable("SteamAppId", "753641");
		Environment.SetEnvironmentVariable("SteamGameId", "753641");

		if (!SteamAPI.Init())
		{
			Log($"FATAL - SteamAPI.Init() failed. Refer to Valve's documentation.");
			return;
		}

		IpcClient.Connect(port);

		IpcClient.Loop();

		Log("stop");
		SteamAPI.Shutdown();
	}

	public static void Log(object msg) => Console.Out.WriteLine(msg);
}
