using Mirror;
using Mirror.Weaver;
using Mono.Cecil;
using System;
using System.Diagnostics;

namespace MirrorWeaver;

public static class QSBReaderWriterProcessor
{
	/// <summary>
	/// finds usages of the generic read/write methods and generates read/write functions for them.
	/// <para/>
	/// traverses from non generic classes up thru base types
	/// in order to replace generic parameters with their corresponding generic arguments.
	/// </summary>
	public static bool Process(ModuleDefinition module, Writers writers, Readers readers, ref bool weavingFailed)
	{
		var sw = Stopwatch.StartNew();

		var NetworkWriter_Write = typeof(NetworkWriter).GetMethod(nameof(NetworkWriter.Write));
		var NetworkReader_Read = typeof(NetworkReader).GetMethod(nameof(NetworkReader.Read));

		var count = 0;
		foreach (var type in module.GetTypes())
		{
			if (type.HasGenericParameters)
			{
				continue;
			}

			TypeReference currentType = type;
			while (currentType != null)
			{
				var currentTypeDef = currentType.Resolve();
				foreach (var method in currentTypeDef.Methods)
				{
					if (!method.HasBody)
					{
						continue;
					}

					foreach (var instruction in method.Body.Instructions)
					{
						if (instruction.Operand is not GenericInstanceMethod calledMethod)
						{
							continue;
						}

						if (calledMethod.DeclaringType.FullName == NetworkWriter_Write.DeclaringType.FullName &&
						    calledMethod.Name == NetworkWriter_Write.Name)
						{
							var argType = calledMethod.GenericArguments[0];

							if (currentType is GenericInstanceType genericInstanceType &&
								argType is GenericParameter genericParameter &&
								genericParameter.Owner == currentTypeDef)
							{
								argType = genericInstanceType.GenericArguments[genericParameter.Position];
							}

							writers.GetWriteFunc(argType, ref weavingFailed);
							count++;
						}
						else if (calledMethod.DeclaringType.FullName == NetworkReader_Read.DeclaringType.FullName &&
						         calledMethod.Name == NetworkReader_Read.Name)
						{
							var argType = calledMethod.GenericArguments[0];

							if (currentType is GenericInstanceType genericInstanceType &&
							    argType is GenericParameter genericParameter &&
							    genericParameter.Owner == currentTypeDef)
							{
								argType = genericInstanceType.GenericArguments[genericParameter.Position];
							}

							readers.GetReadFunc(argType, ref weavingFailed);
							count++;
						}
					}
				}

				currentType = currentTypeDef.BaseType?.ApplyGenericParameters(currentType);
			}
		}

		Console.WriteLine($"got/generated {count} read/write funcs in {sw.ElapsedMilliseconds} ms");
		return count > 0;
	}
}