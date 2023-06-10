using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EpicRerouter.ExeSide;

public static class Program
{
	public static string ProductName;
	public static string Version;

	private static void Main(string[] args)
	{
		ProductName = args[0];
		Log($"product name = {ProductName}");
		Version = args[1];
		Log($"version = {Version}");
		var managedDir = args[2];
		Log($"managed dir = {managedDir}");
		var gameArgs = args.Skip(3).ToArray();
		Log($"game args = {string.Join(", ", gameArgs)}");

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

			while (EpicEntitlementRetriever.GetOwnershipStatus() == EntitlementsManager.AsyncOwnershipStatus.NotReady)
			{
				EpicPlatformManager.Tick();
			}
		}
		finally
		{
			EpicEntitlementRetriever.Uninit();
			EpicPlatformManager.Uninit();

			Environment.Exit((int)EpicEntitlementRetriever.GetOwnershipStatus());
		}
	}

	public static void Log(object msg) => Console.Error.WriteLine(msg);
}