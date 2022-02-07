using HarmonyLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using static EntitlementsManager;

namespace EpicRerouter.ExeSide
{
	public static class Program
	{
		public static string ProductName;
		public static string Version;

		private static void Main(string[] args)
		{
			ProductName = args[1];
			Console.WriteLine($"product name = {ProductName}");
			Version = args[2];
			Console.WriteLine($"version = {Version}");

			var assemblyDirs = args.Skip(2).ToArray();
			Console.WriteLine($"assembly dirs = {assemblyDirs.Join()}");
			Console.WriteLine();

			AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
			{
				var name = new AssemblyName(e.Name).Name + ".dll";
				foreach (var dir in assemblyDirs)
				{
					var path = Path.Combine(dir, name);
					if (File.Exists(path))
					{
						return Assembly.LoadFile(path);
					}
				}

				return null;
			};

			Go();
		}

		private static void Go()
		{
			try
			{
				EpicPlatformManager.Init();
				EpicEntitlementRetriever.Init();

				while (EpicEntitlementRetriever.GetOwnershipStatus() == AsyncOwnershipStatus.NotReady)
				{
					EpicPlatformManager.Tick();
					Thread.Sleep(100);
				}
			}
			finally
			{
				EpicEntitlementRetriever.Uninit();
				EpicPlatformManager.Uninit();

				Console.WriteLine("all done");
				Console.ReadKey();

				Environment.Exit((int)AsyncOwnershipStatus.NotReady);
			}
		}
	}
}
