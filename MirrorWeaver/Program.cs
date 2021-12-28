using Mirror.Weaver;
using Mono.Cecil;
using System;
using System.IO;

namespace MirrorWeaver
{
	internal class ConsoleLogger : Logger
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
			var log = new ConsoleLogger();
			var weaver = new Weaver(log);

			var qsbDll = args[0];
			if (!File.Exists(qsbDll))
			{
				throw new Exception($"qsb dll could not be located at {qsbDll}!");
			}

			Console.WriteLine("Start weaving process.");
			var assembly = AssemblyDefinition.ReadAssembly(qsbDll);
			weaver.Weave(assembly, new DefaultAssemblyResolver(), out var modified);
			assembly.Write(new WriterParameters { WriteSymbols = true });
			Console.WriteLine($"Finish weaving process. modified = {modified}");
		}
	}
}
