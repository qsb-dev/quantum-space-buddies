using System;
using System.IO;
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
			AppDomain.CurrentDomain.UnhandledException += (_, e) =>
			{
				Console.Error.WriteLine(e.ExceptionObject);
				Console.ReadKey();
			};

			ProductName = args[0];
			Console.WriteLine($"product name = {ProductName}");
			Version = args[1];
			Console.WriteLine($"version = {Version}");
			var managedDir = args[2];
			Console.WriteLine($"managed dir = {managedDir}");
			Console.WriteLine();

			AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
			{
				var name = new AssemblyName(e.Name).Name + ".dll";
				var path = Path.Combine(managedDir, name);
				return File.Exists(path) ? Assembly.LoadFile(path) : null;
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

				Environment.Exit((int)AsyncOwnershipStatus.NotReady);
			}
		}
	}
}
