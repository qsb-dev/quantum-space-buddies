using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ProxyInjector
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var sw = Stopwatch.StartNew();

			var qsbDll = args[0];
			var gameDll = args[1];

			var resolver = new DefaultAssemblyResolver();
			resolver.AddSearchDirectory(Path.GetDirectoryName(gameDll));
			using var qsbModule = ModuleDefinition.ReadModule(qsbDll, new ReaderParameters
			{
				ReadWrite = true,
				ReadSymbols = true,
				AssemblyResolver = resolver
			});
			using var gameModule = ModuleDefinition.ReadModule(gameDll, new ReaderParameters { AssemblyResolver = resolver });

			var count = 0;
			foreach (var td in gameModule.Types)
			{
				if (!td.IsDerivedFrom<MonoBehaviour>() ||
					td.IsAbstract ||
					td.HasGenericParameters)
				{
					continue;
				}

				var proxyTd = new TypeDefinition(td.Namespace, "PROXY_" + td.Name, td.Attributes, qsbModule.ImportReference(td));

				var ctor = td.Methods.First(md => md.Name == ".ctor" && !md.HasParameters);
				var proxyCtor = new MethodDefinition(ctor.Name, ctor.Attributes, qsbModule.ImportReference(ctor.ReturnType));
				proxyCtor.Body = new MethodBody(proxyCtor);
				proxyCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				proxyCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, qsbModule.ImportReference(ctor)));
				proxyCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
				proxyTd.Methods.Add(proxyCtor);

				qsbModule.Types.Add(proxyTd);
				count++;
			}

			qsbModule.Write(new WriterParameters { WriteSymbols = true });

			Console.WriteLine($"injected {count} proxy scripts in {sw.ElapsedMilliseconds} ms");
		}

		private static bool IsDerivedFrom<T>(this TypeDefinition td)
		{
			while (td != null)
			{
				if (td.Is(typeof(T)))
				{
					return true;
				}

				td = td.BaseType?.Resolve();
			}

			return false;
		}
	}
}
