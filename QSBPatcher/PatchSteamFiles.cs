using System;
using System.IO;

namespace QSBPatcher
{
	public static class PatchSteamFiles
	{
		public static void Main(string[] args)
		{
			var basePath = args.Length > 0 ? args[0] : ".";
			var gamePath = AppDomain.CurrentDomain.BaseDirectory;

			var dataPath = GetDataPath(gamePath);
			var pluginsPath = Path.Combine(dataPath, @"Plugins\x86_64");

			var dir = new DirectoryInfo(Path.Combine(basePath, "SteamAPI"));
			var files = dir.GetFiles();
			foreach (var file in files)
			{
				var tempPath = Path.Combine(pluginsPath, file.Name);
				file.CopyTo(tempPath, true);
			}
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

		private static string GetDataDirectoryName()
		{
			var gamePath = AppDomain.CurrentDomain.BaseDirectory;
			return $"{GetExecutableName(gamePath)}_Data";
		}

		private static string GetDataPath(string gamePath)
			=> Path.Combine(gamePath, $"{GetDataDirectoryName()}");
	}
}