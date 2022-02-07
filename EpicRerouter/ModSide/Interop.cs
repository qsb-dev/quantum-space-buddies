using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static EntitlementsManager;
using Debug = UnityEngine.Debug;

namespace EpicRerouter.ModSide
{
	public static class Interop
	{
		public static AsyncOwnershipStatus OwnershipStatus { get; private set; } = AsyncOwnershipStatus.NotReady;

		public static void Go()
		{
			if (typeof(EpicPlatformManager).GetField("_platformInterface", BindingFlags.NonPublic | BindingFlags.Instance) == null)
			{
				Log("not epic. don't reroute");
				// return;
			}

			Log("go");

			Patches.Apply();

			var processPath = Path.Combine(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
				"EpicRerouter.exe"
			);
			Log($"process path = {processPath}");
			var args = new[]
			{
				Application.productName,
				Application.version,
				Path.GetDirectoryName(typeof(EpicPlatformManager).Assembly.Location)
			};
			Log($"args = {args.Join()}");
			var workingDirectory = Path.GetFullPath(Path.Combine(args[2], "..", "Plugins", "x86_64"));
			Log($"working dir = {workingDirectory}");
			var process = Process.Start(new ProcessStartInfo
			{
				FileName = processPath,
				WorkingDirectory = workingDirectory,
				Arguments = args.Join(x => $"\"{x}\"", " "),
				UseShellExecute = false
			});
			process!.WaitForExit();
			OwnershipStatus = (AsyncOwnershipStatus)process.ExitCode;
			Log($"ownership status = {OwnershipStatus}");
		}

		public static void Log(object msg) => Debug.LogError($"[interop] {msg}");
	}
}
