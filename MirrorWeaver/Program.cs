using Mirror.Weaver;
using Mono.Cecil;
using System;
using System.IO;

namespace MirrorWeaver
{
	public class ConsoleLogger : Logger
	{
		public void Warning(string message) => Warning(message, null);

		public void Warning(string message, MemberReference mr)
		{
			if (mr != null) message = $"{message} (at {mr})";

			Console.WriteLine(message);
		}

		public void Error(string message) => Error(message, null);

		public void Error(string message, MemberReference mr)
		{
			if (mr != null) message = $"{message} (at {mr})";

			Console.Error.WriteLine(message);
		}
	}

	public class AssemblyResolver : BaseAssemblyResolver
	{
		public AssemblyResolver(string managedDir) => AddSearchDirectory(managedDir);
	}

	public static class Program
	{
		public static void Main(string[] args)
		{
			var log = new ConsoleLogger();
			var weaver = new Weaver(log);

			var qsbDll = args[0];
			var gameDir = args[1];
			var managedDir = Path.Combine(gameDir, "OuterWilds_Data", "Managed");
			// var weavedQsbDll = $"{qsbDll}.weaved.dll";
			// Console.WriteLine($"{qsbDll} => {weavedQsbDll}");

			var assembly = AssemblyDefinition.ReadAssembly(qsbDll);
			var resolver = new DefaultAssemblyResolver();
			// resolver.AddSearchDirectory(Path.GetDirectoryName(qsbDll));
			// resolver.AddSearchDirectory(managedDir);

			var result = weaver.Weave(assembly, resolver, out _);
			if (!result)
			{
				throw new Exception("weaving failed");
			}

			assembly.Write(qsbDll, new WriterParameters { WriteSymbols = true });
		}
	}
}
