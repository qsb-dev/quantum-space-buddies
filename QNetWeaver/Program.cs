using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QNetWeaver
{
	public class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Start!");

			if (args.Length == 0)
			{
				Log.Error("No args supplied!");
			}

			var unityEngine = args[0];
			var qnetDLL = args[1];
			var unetDLL = args[2];
			var outputDirectory = args[3];
			var assembly = args[4];
			
			Program.CheckDLLPath(unityEngine);
			Program.CheckDLLPath(qnetDLL);
			Program.CheckOutputDirectory(outputDirectory);
			Program.CheckAssemblyPath(assembly);
			Weaver.WeaveAssemblies(assembly, null, null, outputDirectory, unityEngine, qnetDLL, unetDLL);
		}

		private static void CheckDLLPath(string path)
		{
			if (!File.Exists(path))
			{
				throw new Exception("dll could not be located at " + path + "!");
			}
		}

		private static void CheckAssemblies(IEnumerable<string> assemblyPaths)
		{
			foreach (var assemblyPath in assemblyPaths)
			{
				Program.CheckAssemblyPath(assemblyPath);
			}
		}

		private static void CheckAssemblyPath(string assemblyPath)
		{
			if (!File.Exists(assemblyPath))
			{
				throw new Exception("Assembly " + assemblyPath + " does not exist!");
			}
		}

		private static void CheckOutputDirectory(string outputDir)
		{
			if (!Directory.Exists(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}
		}
	}
}
