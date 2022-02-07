using System;
using System.Reflection;
using System.Threading;
using static EntitlementsManager;

namespace EpicRerouter
{
	/// <summary>
	/// runs on process side
	/// </summary>
	internal static class Program
	{
		private static void Main(string[] assemblyLocations)
		{
			try
			{
				foreach (var assemblyLocation in assemblyLocations)
				{
					var assembly = Assembly.LoadFile(assemblyLocation);
					Console.WriteLine($"loaded {assembly}");
				}
			}
			finally
			{
				Console.WriteLine("all done");
				Thread.Sleep(3000);
				Environment.Exit((int)AsyncOwnershipStatus.Owned);
			}
		}
	}
}
