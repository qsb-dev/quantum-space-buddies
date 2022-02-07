using System;
using System.Reflection;
using System.Threading;
using static EntitlementsManager;

namespace EpicRerouter.ProcessSide
{
	/// <summary>
	/// runs on process side
	/// </summary>
	internal static class Program
	{
		public static string ProductName;
		public static string Version;

		private static void Main(string[] args)
		{
			try
			{
				foreach (var assemblyLocation in args)
				{
					var assembly = Assembly.LoadFile(assemblyLocation);
					Console.WriteLine($"loaded {assembly}");
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
