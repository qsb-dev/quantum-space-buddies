using Mirror.Weaver;
using Mono.Cecil;
using System;
using System.IO;
using System.Reflection;

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

	public static class Program
	{
		public static void Main(string[] args)
		{
			var qsbDll = Path.GetFullPath(args[0]);
			var gameDir = Path.GetFullPath(args[1]);

			var qsbDir = Path.GetDirectoryName(qsbDll)!;
			var managedDir = Path.Combine(gameDir, "OuterWilds_Data", "Managed");

			AppDomain.CurrentDomain.AssemblyResolve += (_, eventArgs) =>
			{
				var name = new AssemblyName(eventArgs.Name).Name + ".dll";

				var path = Path.Combine(qsbDir, name);
				if (File.Exists(path))
				{
					return Assembly.LoadFile(path);
				}

				path = Path.Combine(managedDir, name);
				if (File.Exists(path))
				{
					return Assembly.LoadFile(path);
				}

				return null;
			};

			var resolver = new DefaultAssemblyResolver();
			resolver.AddSearchDirectory(qsbDir);
			resolver.AddSearchDirectory(managedDir);
			var assembly = AssemblyDefinition.ReadAssembly(qsbDll, new ReaderParameters
			{
				ReadWrite = true,
				ReadSymbols = true,
				AssemblyResolver = resolver
			});

			var log = new ConsoleLogger();
			var weaver = new Weaver(log);
			if (!weaver.Weave(assembly, resolver, out _))
			{
				throw new Exception("weaving failed");
			}

			assembly.Write(new WriterParameters { WriteSymbols = true });
		}
	}
}
