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

		// cache dlc ownership since the patched function gets called often
		OwnershipStatus = IsDlcOwned() ? EntitlementsManager.AsyncOwnershipStatus.Owned : EntitlementsManager.AsyncOwnershipStatus.NotOwned;
	}

	public static void Log(object msg) => Debug.Log($"[SteamRerouter] {msg}");
	public static void LogError(object msg) => Debug.LogError($"[SteamRerouter] {msg}");

	private static bool IsDlcOwned()
	{
		var ownershipStatus = DoCommand(0) != 0;
		Log($"dlc owned: {ownershipStatus}");
		return ownershipStatus;
	}

	public static void EarnAchivement(Achievements.Type type)
	{
		DoCommand(1, (int)type);
		Log($"earned achievement {type}");
	}

	private static int DoCommand(int type, int arg = default)
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
		process!.WaitForExit();

		Log($"StandardOutput:\n{process.StandardOutput.ReadToEnd()}");
		LogError($"StandardError:\n{process.StandardError.ReadToEnd()}");
		Log($"ExitCode: {process.ExitCode}");

		return process.ExitCode;
	}
}
