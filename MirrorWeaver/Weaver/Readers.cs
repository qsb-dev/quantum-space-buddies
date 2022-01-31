using Mono.Cecil;
using Mono.Cecil.Cil;
// to use Mono.Cecil.Rocks here, we need to 'override references' in the
// Unity.Mirror.CodeGen assembly definition file in the Editor, and add CecilX.Rocks.
// otherwise we get an unknown import exception.
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;

namespace Mirror.Weaver
{
	// not static, because ILPostProcessor is multithreaded
	public class Readers
	{
		// Readers are only for this assembly.
		// can't be used from another assembly, otherwise we will get:
		// "System.ArgumentException: Member ... is declared in another module and needs to be imported"
		private readonly AssemblyDefinition assembly;
		private readonly WeaverTypes weaverTypes;
		private readonly TypeDefinition GeneratedCodeClass;
		private readonly Logger Log;
		private readonly Dictionary<TypeReference, MethodReference> readFuncs =
			new Dictionary<TypeReference, MethodReference>(new TypeReferenceComparer());

		public Readers(AssemblyDefinition assembly, WeaverTypes weaverTypes, TypeDefinition GeneratedCodeClass, Logger Log)
		{
			this.assembly = assembly;
			this.weaverTypes = weaverTypes;
			this.GeneratedCodeClass = GeneratedCodeClass;
			this.Log = Log;
		}

		internal void Register(TypeReference dataType, MethodReference methodReference)
		{
			if (readFuncs.ContainsKey(dataType))
			{
				// TODO enable this again later.
				// Reader has some obsolete functions that were renamed.
				// Don't want weaver warnings for all of them.
				//Log.Warning($"Registering a Read method for {dataType.FullName} when one already exists", methodReference);
			}

			// we need to import type when we Initialize Readers so import here in case it is used anywhere else
			var imported = assembly.MainModule.ImportReference(dataType);
			readFuncs[imported] = methodReference;
		}

		private void RegisterReadFunc(TypeReference typeReference, MethodDefinition newReaderFunc)
		{
			Register(typeReference, newReaderFunc);
			GeneratedCodeClass.Methods.Add(newReaderFunc);
		}

		// Finds existing reader for type, if non exists trys to create one
		public MethodReference GetReadFunc(TypeReference variable, ref bool WeavingFailed)
		{
			if (readFuncs.TryGetValue(variable, out var foundFunc))
			{
				return foundFunc;
			}

			var importedVariable = assembly.MainModule.ImportReference(variable);
			return GenerateReader(importedVariable, ref WeavingFailed);
		}

		private MethodReference GenerateReader(TypeReference variableReference, ref bool WeavingFailed)
		{
			// Arrays are special,  if we resolve them, we get the element type,
			// so the following ifs might choke on it for scriptable objects
			// or other objects that require a custom serializer
			// thus check if it is an array and skip all the checks.
			if (variableReference.IsArray)
			{
				if (variableReference.IsMultidimensionalArray())
				{
					Log.Error($"{variableReference.Name} is an unsupported type. Multidimensional arrays are not supported", variableReference);
					WeavingFailed = true;
					return null;
				}

				return GenerateReadCollection(variableReference, variableReference.GetElementType(), nameof(NetworkReaderExtensions.ReadArray), ref WeavingFailed);
			}

			var variableDefinition = variableReference.Resolve();

			// check if the type is completely invalid
			if (variableDefinition == null)
			{
				Log.Error($"{variableReference.Name} is not a supported type", variableReference);
				WeavingFailed = true;
				return null;
			}
			else if (variableReference.IsByReference)
			{
				// error??
				Log.Error($"Cannot pass type {variableReference.Name} by reference", variableReference);
				WeavingFailed = true;
				return null;
			}

			// use existing func for known types
			if (variableDefinition.IsEnum)
			{
				return GenerateEnumReadFunc(variableReference, ref WeavingFailed);
			}
			else if (variableDefinition.Is(typeof(ArraySegment<>)))
			{
				return GenerateArraySegmentReadFunc(variableReference, ref WeavingFailed);
			}
			else if (variableDefinition.Is(typeof(List<>)))
			{
				var genericInstance = (GenericInstanceType)variableReference;
				var elementType = genericInstance.GenericArguments[0];

				return GenerateReadCollection(variableReference, elementType, nameof(NetworkReaderExtensions.ReadList), ref WeavingFailed);
			}
			else if (variableReference.IsDerivedFrom<NetworkBehaviour>())
			{
				return GetNetworkBehaviourReader(variableReference);
			}

			// check if reader generation is applicable on this type
			if (variableDefinition.IsDerivedFrom<UnityEngine.Component>())
			{
				Log.Error($"Cannot generate reader for component type {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
				WeavingFailed = true;
				return null;
			}
			if (variableReference.Is<UnityEngine.Object>())
			{
				Log.Error($"Cannot generate reader for {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
				WeavingFailed = true;
				return null;
			}
			if (variableReference.Is<UnityEngine.ScriptableObject>())
			{
				Log.Error($"Cannot generate reader for {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
				WeavingFailed = true;
				return null;
			}
			if (variableDefinition.HasGenericParameters)
			{
				Log.Error($"Cannot generate reader for generic variable {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
				WeavingFailed = true;
				return null;
			}
			if (variableDefinition.IsInterface)
			{
				Log.Error($"Cannot generate reader for interface {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
				WeavingFailed = true;
				return null;
			}
			if (variableDefinition.IsAbstract)
			{
				Log.Error($"Cannot generate reader for abstract class {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
				WeavingFailed = true;
				return null;
			}

			return GenerateClassOrStructReadFunction(variableReference, ref WeavingFailed);
		}

		private MethodReference GetNetworkBehaviourReader(TypeReference variableReference)
		{
			// uses generic ReadNetworkBehaviour rather than having weaver create one for each NB
			var generic = weaverTypes.readNetworkBehaviourGeneric;

			var readFunc = generic.MakeGeneric(assembly.MainModule, variableReference);

			// register function so it is added to Reader<T>
			// use Register instead of RegisterWriteFunc because this is not a generated function
			Register(variableReference, readFunc);

			return readFunc;
		}

		private MethodDefinition GenerateEnumReadFunc(TypeReference variable, ref bool WeavingFailed)
		{
			var readerFunc = GenerateReaderFunction(variable);

			var worker = readerFunc.Body.GetILProcessor();

			worker.Emit(OpCodes.Ldarg_0);

			var underlyingType = variable.Resolve().GetEnumUnderlyingType();
			var underlyingFunc = GetReadFunc(underlyingType, ref WeavingFailed);

			worker.Emit(OpCodes.Call, underlyingFunc);
			worker.Emit(OpCodes.Ret);
			return readerFunc;
		}

		private MethodDefinition GenerateArraySegmentReadFunc(TypeReference variable, ref bool WeavingFailed)
		{
			var genericInstance = (GenericInstanceType)variable;
			var elementType = genericInstance.GenericArguments[0];

			var readerFunc = GenerateReaderFunction(variable);

			var worker = readerFunc.Body.GetILProcessor();

			// $array = reader.Read<[T]>()
			var arrayType = elementType.MakeArrayType();
			worker.Emit(OpCodes.Ldarg_0);
			worker.Emit(OpCodes.Call, GetReadFunc(arrayType, ref WeavingFailed));

			// return new ArraySegment<T>($array);
			worker.Emit(OpCodes.Newobj, weaverTypes.ArraySegmentConstructorReference.MakeHostInstanceGeneric(assembly.MainModule, genericInstance));
			worker.Emit(OpCodes.Ret);
			return readerFunc;
		}

		private MethodDefinition GenerateReaderFunction(TypeReference variable)
		{
			var functionName = $"_Read_{variable.FullName}";

			// create new reader for this type
			var readerFunc = new MethodDefinition(functionName,
					MethodAttributes.Public |
					MethodAttributes.Static |
					MethodAttributes.HideBySig,
					variable);

			readerFunc.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, weaverTypes.Import<NetworkReader>()));
			readerFunc.Body.InitLocals = true;
			RegisterReadFunc(variable, readerFunc);

			return readerFunc;
		}

		private MethodDefinition GenerateReadCollection(TypeReference variable, TypeReference elementType, string readerFunction, ref bool WeavingFailed)
		{
			var readerFunc = GenerateReaderFunction(variable);
			// generate readers for the element
			GetReadFunc(elementType, ref WeavingFailed);

			var module = assembly.MainModule;
			var readerExtensions = module.ImportReference(typeof(NetworkReaderExtensions));
			var listReader = Resolvers.ResolveMethod(readerExtensions, assembly, Log, readerFunction, ref WeavingFailed);

			var methodRef = new GenericInstanceMethod(listReader);
			methodRef.GenericArguments.Add(elementType);

			// generates
			// return reader.ReadList<T>();

			var worker = readerFunc.Body.GetILProcessor();
			worker.Emit(OpCodes.Ldarg_0); // reader
			worker.Emit(OpCodes.Call, methodRef); // Read

			worker.Emit(OpCodes.Ret);

			return readerFunc;
		}

		private MethodDefinition GenerateClassOrStructReadFunction(TypeReference variable, ref bool WeavingFailed)
		{
			var readerFunc = GenerateReaderFunction(variable);

			// create local for return value
			readerFunc.Body.Variables.Add(new VariableDefinition(variable));

			var worker = readerFunc.Body.GetILProcessor();

			var td = variable.Resolve();

			if (!td.IsValueType)
			{
				GenerateNullCheck(worker, ref WeavingFailed);
			}

			CreateNew(variable, worker, td, ref WeavingFailed);
			ReadAllFields(variable, worker, ref WeavingFailed);

			worker.Emit(OpCodes.Ldloc_0);
			worker.Emit(OpCodes.Ret);
			return readerFunc;
		}

		private void GenerateNullCheck(ILProcessor worker, ref bool WeavingFailed)
		{
			// if (!reader.ReadBoolean()) {
			//   return null;
			// }
			worker.Emit(OpCodes.Ldarg_0);
			worker.Emit(OpCodes.Call, GetReadFunc(weaverTypes.Import<bool>(), ref WeavingFailed));

			var labelEmptyArray = worker.Create(OpCodes.Nop);
			worker.Emit(OpCodes.Brtrue, labelEmptyArray);
			// return null
			worker.Emit(OpCodes.Ldnull);
			worker.Emit(OpCodes.Ret);
			worker.Append(labelEmptyArray);
		}

		// Initialize the local variable with a new instance
		private void CreateNew(TypeReference variable, ILProcessor worker, TypeDefinition td, ref bool WeavingFailed)
		{
			if (variable.IsValueType)
			{
				// structs are created with Initobj
				worker.Emit(OpCodes.Ldloca, 0);
				worker.Emit(OpCodes.Initobj, variable);
			}
			else if (td.IsDerivedFrom<UnityEngine.ScriptableObject>())
			{
				var genericInstanceMethod = new GenericInstanceMethod(weaverTypes.ScriptableObjectCreateInstanceMethod);
				genericInstanceMethod.GenericArguments.Add(variable);
				worker.Emit(OpCodes.Call, genericInstanceMethod);
				worker.Emit(OpCodes.Stloc_0);
			}
			else
			{
				// classes are created with their constructor
				var ctor = Resolvers.ResolveDefaultPublicCtor(variable);
				if (ctor == null)
				{
					Log.Error($"{variable.Name} can't be deserialized because it has no default constructor. Don't use {variable.Name} in [SyncVar]s, Rpcs, Cmds, etc.", variable);
					WeavingFailed = true;
					return;
				}

				var ctorRef = assembly.MainModule.ImportReference(ctor);

				worker.Emit(OpCodes.Newobj, ctorRef);
				worker.Emit(OpCodes.Stloc_0);
			}
		}

		private void ReadAllFields(TypeReference variable, ILProcessor worker, ref bool WeavingFailed)
		{
			foreach (var field in variable.FindAllPublicFields())
			{
				// mismatched ldloca/ldloc for struct/class combinations is invalid IL, which causes crash at runtime
				var opcode = variable.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
				worker.Emit(opcode, 0);
				var readFunc = GetReadFunc(field.FieldType, ref WeavingFailed);
				if (readFunc != null)
				{
					worker.Emit(OpCodes.Ldarg_0);
					worker.Emit(OpCodes.Call, readFunc);
				}
				else
				{
					Log.Error($"{field.Name} has an unsupported type", field);
					WeavingFailed = true;
				}
				var fieldRef = assembly.MainModule.ImportReference(field);

				worker.Emit(OpCodes.Stfld, fieldRef);
			}
		}

		// Save a delegate for each one of the readers into Reader<T>.read
		internal void InitializeReaders(ILProcessor worker)
		{
			var module = assembly.MainModule;

			var genericReaderClassRef = module.ImportReference(typeof(Reader<>));

			var fieldInfo = typeof(Reader<>).GetField(nameof(Reader<object>.read));
			var fieldRef = module.ImportReference(fieldInfo);
			var networkReaderRef = module.ImportReference(typeof(NetworkReader));
			var funcRef = module.ImportReference(typeof(Func<,>));
			var funcConstructorRef = module.ImportReference(typeof(Func<,>).GetConstructors()[0]);

			foreach (var kvp in readFuncs)
			{
				var targetType = kvp.Key;
				var readFunc = kvp.Value;

				// create a Func<NetworkReader, T> delegate
				worker.Emit(OpCodes.Ldnull);
				worker.Emit(OpCodes.Ldftn, readFunc);
				var funcGenericInstance = funcRef.MakeGenericInstanceType(networkReaderRef, targetType);
				var funcConstructorInstance = funcConstructorRef.MakeHostInstanceGeneric(assembly.MainModule, funcGenericInstance);
				worker.Emit(OpCodes.Newobj, funcConstructorInstance);

				// save it in Reader<T>.read
				var genericInstance = genericReaderClassRef.MakeGenericInstanceType(targetType);
				var specializedField = fieldRef.SpecializeField(assembly.MainModule, genericInstance);
				worker.Emit(OpCodes.Stsfld, specializedField);
			}
		}
	}
}
