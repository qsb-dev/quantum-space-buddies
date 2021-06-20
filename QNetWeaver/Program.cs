using System;
using System.IO;

namespace QNetWeaver
{
	public class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine("Start weaving process.");

			if (args.Length == 0)
			{
				Log.Error("No args supplied!");
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
			Weaver.WeaveAssemblies(assembly, null, null, outputDirectory, unityEngine, qnetDLL, unetDLL);
		}

		private static void CheckDLLPath(string path)
		{
			Console.WriteLine($"Check dll {path} ...");
			if (!File.Exists(path))
			{
				throw new Exception("dll could not be located at " + path + "!");
			}

			Console.WriteLine($"Path OK!");
		}

		private static void CheckAssemblyPath(string assemblyPath)
		{
			Console.WriteLine($"Check assembly path {assemblyPath} ...");
			if (!File.Exists(assemblyPath))
			{
				throw new Exception("Assembly " + assemblyPath + " does not exist!");
			}

			Console.WriteLine($"Assembly Path OK!");
		}

		private static void CheckOutputDirectory(string outputDir)
		{
			Console.WriteLine($"Check output path {outputDir} ...");
			if (!Directory.Exists(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}

			Console.WriteLine($"Output Path OK!");
		}
	}
}
