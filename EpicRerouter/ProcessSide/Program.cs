using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using static EntitlementsManager;

namespace EpicRerouter.ProcessSide
{
	public static class Program
	{
		public static string ProductName;
		public static string Version;

		private static void Main(string[] args)
		{
			try
			{
				ProductName = args[1];
				Console.WriteLine($"product name = {ProductName}");
				Version = args[2];
				Console.WriteLine($"version = {Version}");

				foreach (var assemblyLocation in args.Skip(2))
				{
					var assembly = Assembly.LoadFile(assemblyLocation);
					Console.WriteLine($"loaded {assembly} at {assemblyLocation}");
				}

				Console.WriteLine();

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
				Thread.Sleep(3000);
				Environment.Exit((int)AsyncOwnershipStatus.Owned);
			}
		}
	}
}
