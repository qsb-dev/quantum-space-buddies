using Mirror.Weaver;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;

namespace MirrorWeaver
{
	public class ConsoleLogger : Logger
	{
		public void Warning(string message) => Warning(message, null);

		public void Warning(string message, MemberReference mr)
		{
			if (mr != null)
			{
				message = $"{message} (at {mr})";
			}

			Console.WriteLine(message);
		}

		public void Error(string message) => Error(message, null);

		public void Error(string message, MemberReference mr)
		{
			if (mr != null)
			{
				message = $"{message} (at {mr})";
			}

			Console.Error.WriteLine(message);
		}
	}

	public static class Program
	{
		public static void Main(string[] args)
		{
			var qsbDll = Path.GetFullPath(args[0]);
			var unityDir = Path.GetFullPath(args[1]);

			var qsbDir = Path.GetDirectoryName(qsbDll)!;

			var resolver = new DefaultAssemblyResolver();
			resolver.AddSearchDirectory(qsbDir);
			resolver.AddSearchDirectory(unityDir);
			var assembly = AssemblyDefinition.ReadAssembly(qsbDll, new ReaderParameters
			{
				ReadWrite = true,
				AssemblyResolver = resolver,
				SymbolReaderProvider = new DefaultSymbolReaderProvider(false)
			});

			var log = new ConsoleLogger();
			var weaver = new Weaver(log);
			if (!weaver.Weave(assembly, resolver, out _))
			{
				Environment.Exit(1);
			}

			assembly.Write(new WriterParameters { WriteSymbols = assembly.MainModule.HasSymbols });
		}
	}
}
