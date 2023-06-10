﻿using HarmonyLib;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SteamRerouter.ModSide;

/// <summary>
/// top level file on the mod
/// </summary>
public class Interop : MonoBehaviour
{
	private static Process _process;

	private void Awake()
	{
		Log("awake");

		Harmony.CreateAndPatchAll(typeof(Patches));
		var port = IpcServer.Listen();

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
			port.ToString(),
			Path.Combine(gamePath, "Managed")
		};
		Log($"args = {args.Join()}");
		_process = Process.Start(new ProcessStartInfo
		{
			FileName = processPath,
			WorkingDirectory = workingDirectory,
			Arguments = args.Join(x => $"\"{x}\"", " "),

			UseShellExecute = false,
			CreateNoWindow = false //true
		});

		IpcServer.Accept();
	}

	private void OnDestroy()
	{
		Log("destroy");

		IpcServer.Quit();
		_process.WaitForExit();
		_process.Dispose();
	}

	public static void Log(object msg) => Debug.Log($"[SteamRerouter] {msg}");
}
