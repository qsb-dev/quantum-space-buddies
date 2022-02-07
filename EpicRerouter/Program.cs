using Epic.OnlineServices.Ecom;
using System;
using System.Reflection;
using System.Threading;

namespace EpicRerouter
{
	public static class Program
	{
		public static void Main(string[] assemblyLocations)
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
				Environment.Exit((int)OwnershipStatus.Owned);
			}
		}
	}
}
