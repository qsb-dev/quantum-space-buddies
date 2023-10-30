using System;
using System.IO;

namespace QSBPatcher;

public static class QSBPatcher
{
	public static void Main(string[] args)
	{
		var basePath = args.Length > 0 ? args[0] : ".";
		var gamePath = AppDomain.CurrentDomain.BaseDirectory;

		var steamDLLPath = Path.Combine(basePath, "com.rlabrecque.steamworks.net.dll");

		var managedPath = Path.Combine(gamePath, GetDataPath(gamePath), "Managed");

		File.Copy(steamDLLPath, Path.Combine(managedPath, "com.rlabrecque.steamworks.net.dll"), true);
	}

	private static string GetDataDirectoryName()
	{
		var gamePath = AppDomain.CurrentDomain.BaseDirectory;
		return $"{GetExecutableName(gamePath)}_Data";
	}

	private static string GetDataPath(string gamePath)
	{
		return Path.Combine(gamePath, $"{GetDataDirectoryName()}");
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
}
