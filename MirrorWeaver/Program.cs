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

	internal static class Program
	{
		private static void Main(string[] args)
		{
			var log = new ConsoleLogger();
			var weaver = new Weaver(log);

			Console.WriteLine("Start weaving process.");

			if (args.Length == 0)
			{
				log.Error("No args supplied!");
			}

			var unityEngine = args[0];
			var qnetDLL = args[1];
			var unetDLL = args[2];
			var outputDirectory = args[3];
			var assembly = args[4];

			CheckDLLPath(unityEngine);
			CheckDLLPath(qnetDLL);
			CheckDLLPath(unetDLL);
			CheckOutputDirectory(outputDirectory);
			CheckAssemblyPath(assembly);
			// weaver.WeaveAssemblies(assembly, null, null, outputDirectory, unityEngine, qnetDLL, unetDLL);
			weaver.Weave(null, null, out _);
		}

		private static void CheckDLLPath(string path)
		{
			Console.WriteLine($"Check dll {path} ...");
			if (!File.Exists(path))
			{
				throw new Exception("dll could not be located at " + path + "!");
			}

			Console.WriteLine("Path OK!");
		}

		private static void CheckAssemblyPath(string assemblyPath)
		{
			Console.WriteLine($"Check assembly path {assemblyPath} ...");
			if (!File.Exists(assemblyPath))
			{
				throw new Exception("Assembly " + assemblyPath + " does not exist!");
			}

			Console.WriteLine("Assembly Path OK!");
		}

		private static void CheckOutputDirectory(string outputDir)
		{
			Console.WriteLine($"Check output path {outputDir} ...");
			if (!Directory.Exists(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}

			Console.WriteLine("Output Path OK!");
		}
	}
}
