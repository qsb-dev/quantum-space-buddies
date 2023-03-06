using System;
using System.IO;

namespace QSBPrepatch;

internal class PatchSteamFiles
{
	static void Main(string[] args)
	{
		var basePath = args.Length > 0 ? args[0] : ".";
		var gamePath = AppDomain.CurrentDomain.BaseDirectory;
		var dataPath = GetDataPath(gamePath);
		var managedPath = Path.Combine(dataPath, @"Managed");

		// copy lib files
		var libDir = new DirectoryInfo(Path.Combine(basePath, "lib"));
		var files = libDir.GetFiles();
		foreach (var file in files)
		{
			var tempPath = Path.Combine(managedPath, file.Name);
			file.CopyTo(tempPath, true);
		}

		// create steam_appid.txt
		var appidFile = Path.Combine(gamePath, "steam_appid.txt");
		//File.WriteAllText(appidFile, "480");
	}

	private static string GetExecutableName(string gamePath)
	{
		var executableNames = new[] { "Outer Wilds.exe", "OuterWilds.exe" };
		foreach (var executableName in executableNames)
		{
			var executablePath = Path.Combine(gamePath, executableName);
			if (File.Exists(executablePath))
			{
				return Path.GetFileNameWithoutExtension(executablePath);
			}
		}

		throw new FileNotFoundException($"Outer Wilds exe file not found in {gamePath}");
	}

	private static string GetDataPath(string gamePath)
		=> Path.Combine(gamePath, $"{GetDataDirectoryName()}");

	private static string GetDataDirectoryName()
	{
		var gamePath = AppDomain.CurrentDomain.BaseDirectory;
		return $"{GetExecutableName(gamePath)}_Data";
	}
}
