using HarmonyLib;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SteamRerouter.ModSide;

/// <summary>
/// top level file on the mod
/// </summary>
public static class Interop
{
	public static EntitlementsManager.AsyncOwnershipStatus OwnershipStatus = EntitlementsManager.AsyncOwnershipStatus.NotReady;

	public static void Init()
	{
		Log("init");
		Harmony.CreateAndPatchAll(typeof(Patches));

		// Cache DLC ownership since the patched function gets called often.
		// This won't work if the player buys the DLC mid-game, but too bad!
		OwnershipStatus = IsDlcOwned()
			? EntitlementsManager.AsyncOwnershipStatus.Owned
			: EntitlementsManager.AsyncOwnershipStatus.NotOwned;
	}

	public static void Log(object msg) => Debug.Log($"[SteamRerouter] {msg}");
	public static void LogError(object msg) => Debug.LogError($"[SteamRerouter] {msg}");

	private static bool IsDlcOwned()
	{
		var ownershipStatus = DoCommand(true, 0) != 0;
		Log($"dlc owned: {ownershipStatus}");
		return ownershipStatus;
	}

	public static void EarnAchivement(Achievements.Type type)
	{
		Log($"earn achievement {type}");
		DoCommand(false, 1, (int)type);
	}

	private static int DoCommand(bool waitForExit, int type, int arg = default)
	{
		var processPath = Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
			"SteamRerouter.exe"
		);
		Log($"process path = {processPath}");
		var gamePath = Application.dataPath;
		Log($"game path = {gamePath}");
		var workingDirectory = Path.Combine(gamePath, "Plugins", "x86_64");
		Log($"working dir = {workingDirectory}");
		var args = new[]
		{
			Path.Combine(gamePath, "Managed"),
			type.ToString(),
			arg.ToString()
		};

		Log($"args = {args.Join()}");
		var process = Process.Start(new ProcessStartInfo
		{
			FileName = processPath,
			WorkingDirectory = workingDirectory,
			Arguments = args.Join(x => $"\"{x}\"", " "),

			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		});

		if (waitForExit)
		{
			process!.WaitForExit();

			Log($"StandardOutput:\n{process.StandardOutput.ReadToEnd()}");
			Log($"StandardError:\n{process.StandardError.ReadToEnd()}");
			Log($"ExitCode: {process.ExitCode}");

			return process.ExitCode;
		}

		return -1;
	}
}
