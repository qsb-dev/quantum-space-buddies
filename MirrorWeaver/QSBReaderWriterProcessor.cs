using Mirror;
using Mirror.Weaver;
using Mono.Cecil;

namespace MirrorWeaver
{
	public static class QSBReaderWriterProcessor
	{
		/// <summary>
		/// finds usages of the generic read/write methods and generates read/write functions for them.
		/// <para/>
		/// traverses from non abstract classes up thru base types
		/// in order to replace generic parameters with their corresponding generic arguments.
		/// </summary>
		public static void Process(AssemblyDefinition assembly, Writers writers, Readers readers, ref bool weavingFailed)
		{
			var NetworkWriter_Write = assembly.MainModule.ImportReference(typeof(NetworkWriter).GetMethod(nameof(NetworkWriter.Write)));
			var NetworkReader_Read = assembly.MainModule.ImportReference(typeof(NetworkReader).GetMethod(nameof(NetworkReader.Read)));

			foreach (var type in assembly.MainModule.GetTypes())
			{
				if (type.IsAbstract || type.IsInterface) continue;

				TypeReference currentType = type;
				while (currentType != null)
				{
					foreach (var method in currentType.Resolve().Methods)
					{
						if (!method.HasBody) continue;
						foreach (var instruction in method.Body.Instructions)
						{
							if (instruction.Operand is not GenericInstanceMethod calledMethod) continue;

							if (calledMethod.DeclaringType.Name == NetworkWriter_Write.DeclaringType.Name &&
								calledMethod.Name == NetworkWriter_Write.Name)
							{
								var argType = calledMethod.GenericArguments[0];

								if (argType is GenericParameter genericParameter && genericParameter.Owner == currentType.Resolve())
									argType = ((GenericInstanceType)currentType).GenericArguments[genericParameter.Position];

								writers.GetWriteFunc(argType, ref weavingFailed);
							}
							else if (calledMethod.DeclaringType.Name == NetworkReader_Read.DeclaringType.Name &&
								calledMethod.Name == NetworkReader_Read.Name)
							{
								var argType = calledMethod.GenericArguments[0];

								if (argType is GenericParameter genericParameter && genericParameter.Owner == currentType.Resolve())
									argType = ((GenericInstanceType)currentType).GenericArguments[genericParameter.Position];

								readers.GetReadFunc(argType, ref weavingFailed);
							}
						}
					}

					currentType = currentType.Resolve()?.BaseType;
				}
			}
		}
	}
}
