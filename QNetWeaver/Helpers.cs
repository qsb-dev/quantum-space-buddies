using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QNetWeaver
{
	internal class Helpers
	{
		public static string UnityEngineDLLDirectoryName()
		{
			var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
			return (directoryName == null) ? null : directoryName.Replace("file:\\", "");
		}

		public static ISymbolReaderProvider GetSymbolReaderProvider(string inputFile)
		{
			var text = inputFile.Substring(0, inputFile.Length - 4);
			ISymbolReaderProvider result;
			if (File.Exists(text + ".pdb"))
			{
				Console.WriteLine("Symbols will be read from " + text + ".pdb");
				result = new PdbReaderProvider();
			}
			else if (File.Exists(text + ".dll.mdb"))
			{
				Console.WriteLine("Symbols will be read from " + text + ".dll.mdb");
				result = new MdbReaderProvider();
			}
			else
			{
				Console.WriteLine("No symbols for " + inputFile);
				result = null;
			}
			return result;
		}

		public static bool InheritsFromSyncList(TypeReference typeRef)
		{
			try
			{
				if (typeRef.IsValueType)
				{
					return false;
				}
				foreach (var typeReference in Helpers.ResolveInheritanceHierarchy(typeRef))
				{
					if (typeReference.IsGenericInstance)
					{
						var typeDefinition = typeReference.Resolve();
						if (typeDefinition.HasGenericParameters && typeDefinition.FullName == Weaver.SyncListType.FullName)
						{
							return true;
						}
					}
				}
			}
			catch
			{
			}
			return false;
		}

		public static IEnumerable<TypeReference> ResolveInheritanceHierarchy(TypeReference type)
		{
			if (type.IsValueType)
			{
				yield return type;
				yield return Weaver.valueTypeType;
				yield return Weaver.objectType;
				yield break;
			}
			while (type != null && type.FullName != Weaver.objectType.FullName)
			{
				yield return type;
				try
				{
					var typeDefinition = type.Resolve();
					if (typeDefinition == null)
					{
						break;
					}
					type = typeDefinition.BaseType;
				}
				catch
				{
					break;
				}
			}
			yield return Weaver.objectType;
			yield break;
		}

		public static string DestinationFileFor(string outputDir, string assemblyPath)
		{
			var fileName = Path.GetFileName(assemblyPath);
			return Path.Combine(outputDir, fileName);
		}

		public static string PrettyPrintType(TypeReference type)
		{
			string result;
			if (type.IsGenericInstance)
			{
				var genericInstanceType = (GenericInstanceType)type;
				var text = genericInstanceType.Name.Substring(0, genericInstanceType.Name.Length - 2);
				var text2 = "<";
				var text3 = ", ";
				IEnumerable<TypeReference> genericArguments = genericInstanceType.GenericArguments;
				result = text + text2 + string.Join(text3, Enumerable.ToArray<string>(Enumerable.Select<TypeReference, string>(genericArguments, new Func<TypeReference, string>(Helpers.PrettyPrintType)))) + ">";
			}
			else if (type.HasGenericParameters)
			{
				result = type.Name.Substring(0, type.Name.Length - 2) + "<" + string.Join(", ", Enumerable.ToArray<string>(Enumerable.Select<GenericParameter, string>(type.GenericParameters, (GenericParameter x) => x.Name))) + ">";
			}
			else
			{
				result = type.Name;
			}
			return result;
		}

		public static ReaderParameters ReaderParameters(string assemblyPath, IEnumerable<string> extraPaths, IAssemblyResolver assemblyResolver, string unityEngineDLLPath, string unityUNetDLLPath)
		{
			var readerParameters = new ReaderParameters();
			if (assemblyResolver == null)
			{
				assemblyResolver = new DefaultAssemblyResolver();
			}
			var addSearchDirectoryHelper = new Helpers.AddSearchDirectoryHelper(assemblyResolver);
			addSearchDirectoryHelper.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));
			addSearchDirectoryHelper.AddSearchDirectory(Helpers.UnityEngineDLLDirectoryName());
			addSearchDirectoryHelper.AddSearchDirectory(Path.GetDirectoryName(unityEngineDLLPath));
			addSearchDirectoryHelper.AddSearchDirectory(Path.GetDirectoryName(unityUNetDLLPath));
			if (extraPaths != null)
			{
				foreach (var directory in extraPaths)
				{
					addSearchDirectoryHelper.AddSearchDirectory(directory);
				}
			}
			readerParameters.AssemblyResolver = assemblyResolver;
			readerParameters.SymbolReaderProvider = Helpers.GetSymbolReaderProvider(assemblyPath);
			return readerParameters;
		}

		public static WriterParameters GetWriterParameters(ReaderParameters readParams)
		{
			var writerParameters = new WriterParameters();
			if (readParams.SymbolReaderProvider is PdbReaderProvider)
			{
				writerParameters.SymbolWriterProvider = new PdbWriterProvider();
			}
			else if (readParams.SymbolReaderProvider is MdbReaderProvider)
			{
				writerParameters.SymbolWriterProvider = new MdbWriterProvider();
			}
			return writerParameters;
		}

		public static TypeReference MakeGenericType(TypeReference self, params TypeReference[] arguments)
		{
			if (self.GenericParameters.Count != arguments.Length)
			{
				throw new ArgumentException();
			}
			var genericInstanceType = new GenericInstanceType(self);
			foreach (var item in arguments)
			{
				genericInstanceType.GenericArguments.Add(item);
			}
			return genericInstanceType;
		}

		public static MethodReference MakeHostInstanceGeneric(MethodReference self, params TypeReference[] arguments)
		{
			var methodReference = new MethodReference(self.Name, self.ReturnType, Helpers.MakeGenericType(self.DeclaringType, arguments))
			{
				HasThis = self.HasThis,
				ExplicitThis = self.ExplicitThis,
				CallingConvention = self.CallingConvention
			};
			foreach (var parameterDefinition in self.Parameters)
			{
				methodReference.Parameters.Add(new ParameterDefinition(parameterDefinition.ParameterType));
			}
			foreach (var genericParameter in self.GenericParameters)
			{
				methodReference.GenericParameters.Add(new GenericParameter(genericParameter.Name, methodReference));
			}
			return methodReference;
		}

		private class AddSearchDirectoryHelper
		{
			public AddSearchDirectoryHelper(IAssemblyResolver assemblyResolver)
			{
				var method = assemblyResolver.GetType().GetMethod("AddSearchDirectory", (BindingFlags)20, null, new Type[]
				{
					typeof(string)
				}, null);
				if (method == null)
				{
					throw new Exception("Assembly resolver doesn't implement AddSearchDirectory method.");
				}
				this._addSearchDirectory = (Helpers.AddSearchDirectoryHelper.AddSearchDirectoryDelegate)Delegate.CreateDelegate(typeof(Helpers.AddSearchDirectoryHelper.AddSearchDirectoryDelegate), assemblyResolver, method);
			}

			public void AddSearchDirectory(string directory)
			{
				this._addSearchDirectory(directory);
			}

			private readonly Helpers.AddSearchDirectoryHelper.AddSearchDirectoryDelegate _addSearchDirectory;

			private delegate void AddSearchDirectoryDelegate(string directory);
		}
	}
}
