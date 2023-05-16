using Mirror.Weaver;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;

namespace MirrorWeaver;

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
		var qsbDll = args[0];

		var resolver = new DefaultAssemblyResolver();
		resolver.AddSearchDirectory(Path.GetDirectoryName(qsbDll));
		var assembly = AssemblyDefinition.ReadAssembly(qsbDll, new ReaderParameters
		{
			ReadWrite = true,
			AssemblyResolver = resolver,
			SymbolReaderProvider = new DefaultSymbolReaderProvider(false)
		});

		var log = new ConsoleLogger();
		var weaver = new Weaver(log);
		if (!weaver.Weave(assembly, resolver, out var modified))
		{
			Environment.Exit(1);
		}

		if (modified)
		{
			assembly.Write(new WriterParameters { WriteSymbols = assembly.MainModule.HasSymbols });
		}
	}
}
