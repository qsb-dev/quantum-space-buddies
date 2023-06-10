using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SteamRerouter.ModSide;

public static class Interop
{
	public static EntitlementsManager.AsyncOwnershipStatus OwnershipStatus = EntitlementsManager.AsyncOwnershipStatus.NotReady;

	public static void Go()
	{
		Log("go");

		Patches.Apply();

		var processPath = Path.Combine(
			Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
			"SteamRerouter.exe"
		);
		Log($"process path = {processPath}");
		var gamePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(EpicPlatformManager).Assembly.Location)!, ".."));
		Log($"game path = {gamePath}");
		var workingDirectory = Path.Combine(gamePath, "Plugins", "x86_64");
		Log($"working dir = {workingDirectory}");
		var args = new[]
		{
			Application.productName,
			Application.version,
			Path.Combine(gamePath, "Managed")
		};
		Log($"args = {args.Join()}");
		var gameArgs = Environment.GetCommandLineArgs();
		Log($"game args = {gameArgs.Join()}");
		var process = Process.Start(new ProcessStartInfo
		{
			FileName = processPath,
			WorkingDirectory = workingDirectory,
			Arguments = args
				.Concat(gameArgs)
				.Join(x => $"\"{x}\"", " "),

			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		});
		process!.WaitForExit();
		OwnershipStatus = (EntitlementsManager.AsyncOwnershipStatus)process.ExitCode;
		Log($"ownership status = {OwnershipStatus}");

		Log($"output:\n{process.StandardOutput.ReadToEnd()}");
		Log($"error:\n{process.StandardError.ReadToEnd()}");
	}

	public static void Log(object msg) => Debug.Log($"[SteamRerouter] {msg}");
}