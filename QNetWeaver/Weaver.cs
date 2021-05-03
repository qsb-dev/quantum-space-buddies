using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace QNetWeaver
{
	internal class Weaver
	{
		public static void ResetRecursionCount() => s_RecursionCount = 0;

		public static bool CanBeResolved(TypeReference parent)
		{
			while (parent != null)
			{
				bool result;
				if (parent.Scope.Name == "Windows")
				{
					result = false;
				}
				else
				{
					if (!(parent.Scope.Name == "mscorlib"))
					{
						try
						{
							parent = parent.Resolve().BaseType;
						}
						catch
						{
							return false;
						}
						continue;
					}
					var typeDefinition = parent.Resolve();
					result = (typeDefinition != null);
				}
				return result;
			}
			return true;
		}

		public static bool IsArrayType(TypeReference variable) => (!variable.IsArray || !((ArrayType)variable).ElementType.IsArray) && (!variable.IsArray || ((ArrayType)variable).Rank <= 1);

		public static void DLog(TypeDefinition td, string fmt, params object[] args)
		{
			if (m_DebugFlag)
			{
				Console.WriteLine("[" + td.Name + "] " + string.Format(fmt, args));
			}
		}

		public static int GetSyncVarStart(string className)
		{
			int result;
			if (lists.numSyncVars.ContainsKey(className))
			{
				var num = lists.numSyncVars[className];
				result = num;
			}
			else
			{
				result = 0;
			}
			return result;
		}

		public static void SetNumSyncVars(string className, int num) => lists.numSyncVars[className] = num;

		public static MethodReference GetWriteFunc(TypeReference variable)
		{
			MethodReference result;
			if (s_RecursionCount++ > 128)
			{
				Log.Error("GetWriteFunc recursion depth exceeded for " + variable.Name + ". Check for self-referencing member variables.");
				fail = true;
				result = null;
			}
			else
			{
				if (lists.writeFuncs.ContainsKey(variable.FullName))
				{
					var methodReference = lists.writeFuncs[variable.FullName];
					if (methodReference.Parameters[0].ParameterType.IsArray == variable.IsArray)
					{
						return methodReference;
					}
				}
				if (variable.IsByReference)
				{
					Log.Error("GetWriteFunc variable.IsByReference error.");
					result = null;
				}
				else
				{
					MethodDefinition methodDefinition;
					if (variable.IsArray)
					{
						var elementType = variable.GetElementType();
						var writeFunc = GetWriteFunc(elementType);
						if (writeFunc == null)
						{
							return null;
						}
						methodDefinition = GenerateArrayWriteFunc(variable, writeFunc);
					}
					else
					{
						if (variable.Resolve().IsEnum)
						{
							return NetworkWriterWriteInt32;
						}
						methodDefinition = GenerateWriterFunction(variable);
					}
					if (methodDefinition == null)
					{
						result = null;
					}
					else
					{
						RegisterWriteFunc(variable.FullName, methodDefinition);
						result = methodDefinition;
					}
				}
			}
			return result;
		}

		public static void RegisterWriteFunc(string name, MethodDefinition newWriterFunc)
		{
			lists.writeFuncs[name] = newWriterFunc;
			lists.generatedWriteFunctions.Add(newWriterFunc);
			ConfirmGeneratedCodeClass(scriptDef.MainModule);
			lists.generateContainerClass.Methods.Add(newWriterFunc);
		}

		public static MethodReference GetReadByReferenceFunc(TypeReference variable)
		{
			MethodReference result;
			if (lists.readByReferenceFuncs.ContainsKey(variable.FullName))
			{
				result = lists.readByReferenceFuncs[variable.FullName];
			}
			else
			{
				result = null;
			}
			return result;
		}

		public static MethodReference GetReadFunc(TypeReference variable)
		{
			if (lists.readFuncs.ContainsKey(variable.FullName))
			{
				var methodReference = lists.readFuncs[variable.FullName];
				if (methodReference.ReturnType.IsArray == variable.IsArray)
				{
					return methodReference;
				}
			}
			var typeDefinition = variable.Resolve();
			MethodReference result;
			if (typeDefinition == null)
			{
				Log.Error("GetReadFunc unsupported type " + variable.FullName);
				result = null;
			}
			else if (variable.IsByReference)
			{
				Log.Error("GetReadFunc variable.IsByReference error.");
				result = null;
			}
			else
			{
				MethodDefinition methodDefinition;
				if (variable.IsArray)
				{
					var elementType = variable.GetElementType();
					var readFunc = GetReadFunc(elementType);
					if (readFunc == null)
					{
						return null;
					}
					methodDefinition = GenerateArrayReadFunc(variable, readFunc);
				}
				else
				{
					if (typeDefinition.IsEnum)
					{
						return NetworkReaderReadInt32;
					}
					methodDefinition = GenerateReadFunction(variable);
				}
				if (methodDefinition == null)
				{
					Log.Error("GetReadFunc unable to generate function for:" + variable.FullName);
					result = null;
				}
				else
				{
					RegisterReadFunc(variable.FullName, methodDefinition);
					result = methodDefinition;
				}
			}
			return result;
		}

		public static void RegisterReadByReferenceFunc(string name, MethodDefinition newReaderFunc)
		{
			lists.readByReferenceFuncs[name] = newReaderFunc;
			lists.generatedReadFunctions.Add(newReaderFunc);
			ConfirmGeneratedCodeClass(scriptDef.MainModule);
			lists.generateContainerClass.Methods.Add(newReaderFunc);
		}

		public static void RegisterReadFunc(string name, MethodDefinition newReaderFunc)
		{
			lists.readFuncs[name] = newReaderFunc;
			lists.generatedReadFunctions.Add(newReaderFunc);
			ConfirmGeneratedCodeClass(scriptDef.MainModule);
			lists.generateContainerClass.Methods.Add(newReaderFunc);
		}

		private static MethodDefinition GenerateArrayReadFunc(TypeReference variable, MethodReference elementReadFunc)
		{
			MethodDefinition result;
			if (!IsArrayType(variable))
			{
				Log.Error(variable.FullName + " is an unsupported array type. Jagged and multidimensional arrays are not supported");
				result = null;
			}
			else
			{
				var text = "_ReadArray" + variable.GetElementType().Name + "_";
				if (variable.DeclaringType != null)
				{
					text += variable.DeclaringType.Name;
				}
				else
				{
					text += "None";
				}
				var methodDefinition = new MethodDefinition(text, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, variable);
				methodDefinition.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, scriptDef.MainModule.ImportReference(NetworkReaderType)));
				methodDefinition.Body.Variables.Add(new VariableDefinition(int32Type));
				methodDefinition.Body.Variables.Add(new VariableDefinition(variable));
				methodDefinition.Body.Variables.Add(new VariableDefinition(int32Type));
				methodDefinition.Body.InitLocals = true;
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, NetworkReadUInt16));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				var instruction = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brtrue, instruction));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Newarr, variable.GetElementType()));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				ilprocessor.Append(instruction);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Newarr, variable.GetElementType()));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_2));
				var instruction2 = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Br, instruction2));
				var instruction3 = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(instruction3);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldelema, variable.GetElementType()));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, elementReadFunc));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stobj, variable.GetElementType()));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Add));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_2));
				ilprocessor.Append(instruction2);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Blt, instruction3));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				result = methodDefinition;
			}
			return result;
		}

		private static MethodDefinition GenerateArrayWriteFunc(TypeReference variable, MethodReference elementWriteFunc)
		{
			MethodDefinition result;
			if (!IsArrayType(variable))
			{
				Log.Error(variable.FullName + " is an unsupported array type. Jagged and multidimensional arrays are not supported");
				result = null;
			}
			else
			{
				var text = "_WriteArray" + variable.GetElementType().Name + "_";
				if (variable.DeclaringType != null)
				{
					text += variable.DeclaringType.Name;
				}
				else
				{
					text += "None";
				}
				var methodDefinition = new MethodDefinition(text, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, voidType);
				methodDefinition.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, scriptDef.MainModule.ImportReference(NetworkWriterType)));
				methodDefinition.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, scriptDef.MainModule.ImportReference(variable)));
				methodDefinition.Body.Variables.Add(new VariableDefinition(uint16Type));
				methodDefinition.Body.Variables.Add(new VariableDefinition(uint16Type));
				methodDefinition.Body.InitLocals = true;
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				var instruction = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brtrue, instruction));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, NetworkWriteUInt16));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				ilprocessor.Append(instruction);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldlen));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Conv_I4));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Conv_U2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, NetworkWriteUInt16));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_1));
				var instruction2 = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Br, instruction2));
				var instruction3 = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(instruction3);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldelema, variable.GetElementType()));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldobj, variable.GetElementType()));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, elementWriteFunc));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Add));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Conv_U2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_1));
				ilprocessor.Append(instruction2);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldlen));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Conv_I4));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Blt, instruction3));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				result = methodDefinition;
			}
			return result;
		}

		private static MethodDefinition GenerateWriterFunction(TypeReference variable)
		{
			MethodDefinition result;
			if (!IsValidTypeToGenerate(variable.Resolve()))
			{
				result = null;
			}
			else
			{
				var text = "_Write" + variable.Name + "_";
				if (variable.DeclaringType != null)
				{
					text += variable.DeclaringType.Name;
				}
				else
				{
					text += "None";
				}
				var methodDefinition = new MethodDefinition(text, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, voidType);
				methodDefinition.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, scriptDef.MainModule.ImportReference(NetworkWriterType)));
				methodDefinition.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, scriptDef.MainModule.ImportReference(variable)));
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				var num = 0U;
				foreach (var fieldDefinition in variable.Resolve().Fields)
				{
					if (!fieldDefinition.IsStatic && !fieldDefinition.IsPrivate)
					{
						if (fieldDefinition.FieldType.Resolve().HasGenericParameters)
						{
							fail = true;
							Log.Error(string.Concat(new object[]
							{
								"WriteReadFunc for ",
								fieldDefinition.Name,
								" [",
								fieldDefinition.FieldType,
								"/",
								fieldDefinition.FieldType.FullName,
								"]. Cannot have generic parameters."
							}));
							return null;
						}
						if (fieldDefinition.FieldType.Resolve().IsInterface)
						{
							fail = true;
							Log.Error(string.Concat(new object[]
							{
								"WriteReadFunc for ",
								fieldDefinition.Name,
								" [",
								fieldDefinition.FieldType,
								"/",
								fieldDefinition.FieldType.FullName,
								"]. Cannot be an interface."
							}));
							return null;
						}
						var writeFunc = GetWriteFunc(fieldDefinition.FieldType);
						if (writeFunc == null)
						{
							Log.Error(string.Concat(new object[]
							{
								"WriteReadFunc for ",
								fieldDefinition.Name,
								" type ",
								fieldDefinition.FieldType,
								" no supported"
							}));
							fail = true;
							return null;
						}
						num += 1U;
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldDefinition));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Call, writeFunc));
					}
				}
				if (num == 0U)
				{
					Log.Warning("The class / struct " + variable.Name + " has no public or non-static fields to serialize");
				}
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				result = methodDefinition;
			}
			return result;
		}

		private static MethodDefinition GenerateReadFunction(TypeReference variable)
		{
			MethodDefinition result;
			if (!IsValidTypeToGenerate(variable.Resolve()))
			{
				result = null;
			}
			else
			{
				var text = "_Read" + variable.Name + "_";
				if (variable.DeclaringType != null)
				{
					text += variable.DeclaringType.Name;
				}
				else
				{
					text += "None";
				}
				var methodDefinition = new MethodDefinition(text, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, variable);
				methodDefinition.Body.Variables.Add(new VariableDefinition(variable));
				methodDefinition.Body.InitLocals = true;
				methodDefinition.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, scriptDef.MainModule.ImportReference(NetworkReaderType)));
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				if (variable.IsValueType)
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloca, 0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Initobj, variable));
				}
				else
				{
					var methodDefinition2 = ResolveDefaultPublicCtor(variable);
					if (methodDefinition2 == null)
					{
						Log.Error("The class " + variable.Name + " has no default constructor or it's private, aborting.");
						return null;
					}
					ilprocessor.Append(ilprocessor.Create(OpCodes.Newobj, methodDefinition2));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_0));
				}
				var num = 0U;
				foreach (var fieldDefinition in variable.Resolve().Fields)
				{
					if (!fieldDefinition.IsStatic && !fieldDefinition.IsPrivate)
					{
						if (variable.IsValueType)
						{
							ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloca, 0));
						}
						else
						{
							ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc, 0));
						}
						var readFunc = GetReadFunc(fieldDefinition.FieldType);
						if (readFunc == null)
						{
							Log.Error(string.Concat(new object[]
							{
								"GetReadFunc for ",
								fieldDefinition.Name,
								" type ",
								fieldDefinition.FieldType,
								" no supported"
							}));
							fail = true;
							return null;
						}
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Call, readFunc));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Stfld, fieldDefinition));
						num += 1U;
					}
				}
				if (num == 0U)
				{
					Log.Warning("The class / struct " + variable.Name + " has no public or non-static fields to serialize");
				}
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				result = methodDefinition;
			}
			return result;
		}

		private static Instruction GetEventLoadInstruction(ModuleDefinition moduleDef, TypeDefinition td, MethodDefinition md, int iCount, FieldReference foundEventField)
		{
			while (iCount > 0)
			{
				iCount--;
				var instruction = md.Body.Instructions[iCount];
				if (instruction.OpCode == OpCodes.Ldfld)
				{
					if (instruction.Operand == foundEventField)
					{
						DLog(td, "    " + instruction.Operand, new object[0]);
						return instruction;
					}
				}
			}
			return null;
		}

		private static void ProcessInstructionMethod(ModuleDefinition moduleDef, TypeDefinition td, MethodDefinition md, Instruction instr, MethodReference opMethodRef, int iCount)
		{
			if (opMethodRef.Name == "Invoke")
			{
				var flag = false;
				while (iCount > 0 && !flag)
				{
					iCount--;
					var instruction = md.Body.Instructions[iCount];
					if (instruction.OpCode == OpCodes.Ldfld)
					{
						var fieldReference = instruction.Operand as FieldReference;
						for (var i = 0; i < lists.replacedEvents.Count; i++)
						{
							var eventDefinition = lists.replacedEvents[i];
							if (eventDefinition.Name == fieldReference.Name)
							{
								instr.Operand = lists.replacementEvents[i];
								instruction.OpCode = OpCodes.Nop;
								flag = true;
								break;
							}
						}
					}
				}
			}
			else if (lists.replacementMethodNames.Contains(opMethodRef.FullName))
			{
				for (var j = 0; j < lists.replacedMethods.Count; j++)
				{
					var methodDefinition = lists.replacedMethods[j];
					if (opMethodRef.FullName == methodDefinition.FullName)
					{
						instr.Operand = lists.replacementMethods[j];
						break;
					}
				}
			}
		}

		private static void ConfirmGeneratedCodeClass(ModuleDefinition moduleDef)
		{
			if (lists.generateContainerClass == null)
			{
				lists.generateContainerClass = new TypeDefinition("Unity", "GeneratedNetworkCode", TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.BeforeFieldInit, objectType);
				var methodDefinition = new MethodDefinition(".ctor", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, voidType);
				methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Call, ResolveMethod(objectType, ".ctor")));
				methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
				lists.generateContainerClass.Methods.Add(methodDefinition);
			}
		}

		private static void ProcessInstructionField(TypeDefinition td, MethodDefinition md, Instruction i, FieldDefinition opField)
		{
			if (!(md.Name == ".ctor") && !(md.Name == "OnDeserialize"))
			{
				for (var j = 0; j < lists.replacedFields.Count; j++)
				{
					var fieldDefinition = lists.replacedFields[j];
					if (opField == fieldDefinition)
					{
						i.OpCode = OpCodes.Call;
						i.Operand = lists.replacementProperties[j];
						break;
					}
				}
			}
		}

		private static void ProcessInstruction(ModuleDefinition moduleDef, TypeDefinition td, MethodDefinition md, Instruction i, int iCount)
		{
			if (i.OpCode == OpCodes.Call || i.OpCode == OpCodes.Callvirt)
			{
				var methodReference = i.Operand as MethodReference;
				if (methodReference != null)
				{
					ProcessInstructionMethod(moduleDef, td, md, i, methodReference, iCount);
				}
			}
			if (i.OpCode == OpCodes.Stfld)
			{
				var fieldDefinition = i.Operand as FieldDefinition;
				if (fieldDefinition != null)
				{
					ProcessInstructionField(td, md, i, fieldDefinition);
				}
			}
		}

		private static void InjectGuardParameters(MethodDefinition md, ILProcessor worker, Instruction top)
		{
			var num = (!md.Resolve().IsStatic) ? 1 : 0;
			for (var i = 0; i < md.Parameters.Count; i++)
			{
				var parameterDefinition = md.Parameters[i];
				if (parameterDefinition.IsOut)
				{
					var elementType = parameterDefinition.ParameterType.GetElementType();
					if (elementType.IsPrimitive)
					{
						worker.InsertBefore(top, worker.Create(OpCodes.Ldarg, i + num));
						worker.InsertBefore(top, worker.Create(OpCodes.Ldc_I4_0));
						worker.InsertBefore(top, worker.Create(OpCodes.Stind_I4));
					}
					else
					{
						md.Body.Variables.Add(new VariableDefinition(elementType));
						md.Body.InitLocals = true;
						worker.InsertBefore(top, worker.Create(OpCodes.Ldarg, i + num));
						worker.InsertBefore(top, worker.Create(OpCodes.Ldloca_S, (byte)(md.Body.Variables.Count - 1)));
						worker.InsertBefore(top, worker.Create(OpCodes.Initobj, elementType));
						worker.InsertBefore(top, worker.Create(OpCodes.Ldloc, md.Body.Variables.Count - 1));
						worker.InsertBefore(top, worker.Create(OpCodes.Stobj, elementType));
					}
				}
			}
		}

		private static void InjectGuardReturnValue(MethodDefinition md, ILProcessor worker, Instruction top)
		{
			if (md.ReturnType.FullName != voidType.FullName)
			{
				if (md.ReturnType.IsPrimitive)
				{
					worker.InsertBefore(top, worker.Create(OpCodes.Ldc_I4_0));
				}
				else
				{
					md.Body.Variables.Add(new VariableDefinition(md.ReturnType));
					md.Body.InitLocals = true;
					worker.InsertBefore(top, worker.Create(OpCodes.Ldloca_S, (byte)(md.Body.Variables.Count - 1)));
					worker.InsertBefore(top, worker.Create(OpCodes.Initobj, md.ReturnType));
					worker.InsertBefore(top, worker.Create(OpCodes.Ldloc, md.Body.Variables.Count - 1));
				}
			}
		}

		private static void InjectServerGuard(ModuleDefinition moduleDef, TypeDefinition td, MethodDefinition md, bool logWarning)
		{
			if (!IsNetworkBehaviour(td))
			{
				Log.Error("[Server] guard on non-NetworkBehaviour script at [" + md.FullName + "]");
			}
			else
			{
				var ilprocessor = md.Body.GetILProcessor();
				var instruction = md.Body.Instructions[0];
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Call, NetworkServerGetActive));
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Brtrue, instruction));
				if (logWarning)
				{
					ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Ldstr, "[Server] function '" + md.FullName + "' called on client"));
					ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Call, logWarningReference));
				}
				InjectGuardParameters(md, ilprocessor, instruction);
				InjectGuardReturnValue(md, ilprocessor, instruction);
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Ret));
			}
		}

		private static void InjectClientGuard(ModuleDefinition moduleDef, TypeDefinition td, MethodDefinition md, bool logWarning)
		{
			if (!IsNetworkBehaviour(td))
			{
				Log.Error("[Client] guard on non-NetworkBehaviour script at [" + md.FullName + "]");
			}
			else
			{
				var ilprocessor = md.Body.GetILProcessor();
				var instruction = md.Body.Instructions[0];
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Call, NetworkClientGetActive));
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Brtrue, instruction));
				if (logWarning)
				{
					ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Ldstr, "[Client] function '" + md.FullName + "' called on server"));
					ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Call, logWarningReference));
				}
				InjectGuardParameters(md, ilprocessor, instruction);
				InjectGuardReturnValue(md, ilprocessor, instruction);
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Ret));
			}
		}

		private static void ProcessSiteMethod(ModuleDefinition moduleDef, TypeDefinition td, MethodDefinition md)
		{
			if (!(md.Name == ".cctor") && !(md.Name == "OnUnserializeVars"))
			{
				var text = md.Name.Substring(0, Math.Min(md.Name.Length, 4));
				if (!(text == "UNet"))
				{
					text = md.Name.Substring(0, Math.Min(md.Name.Length, 7));
					if (!(text == "CallCmd"))
					{
						text = md.Name.Substring(0, Math.Min(md.Name.Length, 9));
						if (!(text == "InvokeCmd") && !(text == "InvokeRpc") && !(text == "InvokeSyn"))
						{
							if (md.Body != null && md.Body.Instructions != null)
							{
								foreach (var customAttribute in md.CustomAttributes)
								{
									if (customAttribute.Constructor.DeclaringType.ToString() == "UnityEngine.Networking.ServerAttribute")
									{
										InjectServerGuard(moduleDef, td, md, true);
									}
									else if (customAttribute.Constructor.DeclaringType.ToString() == "UnityEngine.Networking.ServerCallbackAttribute")
									{
										InjectServerGuard(moduleDef, td, md, false);
									}
									else if (customAttribute.Constructor.DeclaringType.ToString() == "UnityEngine.Networking.ClientAttribute")
									{
										InjectClientGuard(moduleDef, td, md, true);
									}
									else if (customAttribute.Constructor.DeclaringType.ToString() == "UnityEngine.Networking.ClientCallbackAttribute")
									{
										InjectClientGuard(moduleDef, td, md, false);
									}
								}
								var num = 0;
								foreach (var i in md.Body.Instructions)
								{
									ProcessInstruction(moduleDef, td, md, i, num);
									num++;
								}
							}
						}
					}
				}
			}
		}

		private static void ProcessSiteClass(ModuleDefinition moduleDef, TypeDefinition td)
		{
			foreach (var md in td.Methods)
			{
				ProcessSiteMethod(moduleDef, td, md);
			}
			foreach (var td2 in td.NestedTypes)
			{
				ProcessSiteClass(moduleDef, td2);
			}
		}

		private static void ProcessSitesModule(ModuleDefinition moduleDef)
		{
			var now = DateTime.Now;
			foreach (var typeDefinition in moduleDef.Types)
			{
				if (typeDefinition.IsClass)
				{
					ProcessSiteClass(moduleDef, typeDefinition);
				}
			}
			if (lists.generateContainerClass != null)
			{
				moduleDef.Types.Add(lists.generateContainerClass);
				scriptDef.MainModule.ImportReference(lists.generateContainerClass);
				foreach (var method in lists.generatedReadFunctions)
				{
					scriptDef.MainModule.ImportReference(method);
				}
				foreach (var method2 in lists.generatedWriteFunctions)
				{
					scriptDef.MainModule.ImportReference(method2);
				}
			}
			Console.WriteLine(string.Concat(new object[]
			{
				"  ProcessSitesModule ",
				moduleDef.Name,
				" elapsed time:",
				DateTime.Now - now
			}));
		}

		private static void ProcessPropertySites() => ProcessSitesModule(scriptDef.MainModule);

		private static bool ProcessMessageType(TypeDefinition td)
		{
			var messageClassProcessor = new MessageClassProcessor(td);
			messageClassProcessor.Process();
			return true;
		}

		private static bool ProcessSyncListStructType(TypeDefinition td)
		{
			var syncListStructProcessor = new SyncListStructProcessor(td);
			syncListStructProcessor.Process();
			return true;
		}

		private static void ProcessMonoBehaviourType(TypeDefinition td)
		{
			var monoBehaviourProcessor = new MonoBehaviourProcessor(td);
			monoBehaviourProcessor.Process();
		}

		private static bool ProcessNetworkBehaviourType(TypeDefinition td)
		{
			foreach (var methodDefinition in td.Resolve().Methods)
			{
				if (methodDefinition.Name == "UNetVersion")
				{
					DLog(td, " Already processed", new object[0]);
					return false;
				}
			}
			DLog(td, "Found NetworkBehaviour " + td.FullName, new object[0]);
			var networkBehaviourProcessor = new NetworkBehaviourProcessor(td);
			networkBehaviourProcessor.Process();
			return true;
		}

		public static MethodReference ResolveMethod(TypeReference t, string name)
		{
			MethodReference result;
			if (t == null)
			{
				Log.Error("Type missing for " + name);
				fail = true;
				result = null;
			}
			else
			{
				foreach (var methodDefinition in t.Resolve().Methods)
				{
					if (methodDefinition.Name == name)
					{
						return scriptDef.MainModule.ImportReference(methodDefinition);
					}
				}
				Log.Error($"ResolveMethod failed - Couldn't find {name} in {t.Name}");
				foreach (var methodDefinition2 in t.Resolve().Methods)
				{
					Log.Error("- has method " + methodDefinition2.Name);
				}
				fail = true;
				result = null;
			}
			return result;
		}

		private static MethodReference ResolveMethodWithArg(TypeReference t, string name, TypeReference argType)
		{
			foreach (var methodDefinition in t.Resolve().Methods)
			{
				if (methodDefinition.Name == name)
				{
					if (methodDefinition.Parameters.Count == 1)
					{
						if (methodDefinition.Parameters[0].ParameterType.FullName == argType.FullName)
						{
							return scriptDef.MainModule.ImportReference(methodDefinition);
						}
					}
				}
			}
			Log.Error(string.Concat(new object[]
			{
				"ResolveMethodWithArg failed ",
				t.Name,
				"::",
				name,
				" ",
				argType
			}));
			fail = true;
			return null;
		}

		private static MethodDefinition ResolveDefaultPublicCtor(TypeReference variable)
		{
			foreach (var methodDefinition in variable.Resolve().Methods)
			{
				if (methodDefinition.Name == ".ctor" && methodDefinition.Resolve().IsPublic && methodDefinition.Parameters.Count == 0)
				{
					return methodDefinition;
				}
			}
			return null;
		}

		private static GenericInstanceMethod ResolveMethodGeneric(TypeReference t, string name, TypeReference genericType)
		{
			foreach (var methodDefinition in t.Resolve().Methods)
			{
				if (methodDefinition.Name == name)
				{
					if (methodDefinition.Parameters.Count == 0)
					{
						if (methodDefinition.GenericParameters.Count == 1)
						{
							var method = scriptDef.MainModule.ImportReference(methodDefinition);
							var genericInstanceMethod = new GenericInstanceMethod(method);
							genericInstanceMethod.GenericArguments.Add(genericType);
							if (genericInstanceMethod.GenericArguments[0].FullName == genericType.FullName)
							{
								return genericInstanceMethod;
							}
						}
					}
				}
			}
			Log.Error(string.Concat(new object[]
			{
				"ResolveMethodGeneric failed ",
				t.Name,
				"::",
				name,
				" ",
				genericType
			}));
			fail = true;
			return null;
		}

		public static FieldReference ResolveField(TypeReference t, string name)
		{
			foreach (var fieldDefinition in t.Resolve().Fields)
			{
				if (fieldDefinition.Name == name)
				{
					return scriptDef.MainModule.ImportReference(fieldDefinition);
				}
			}
			return null;
		}

		public static MethodReference ResolveProperty(TypeReference t, string name)
		{
			foreach (var propertyDefinition in t.Resolve().Properties)
			{
				if (propertyDefinition.Name == name)
				{
					return scriptDef.MainModule.ImportReference(propertyDefinition.GetMethod);
				}
			}
			Log.Error($"ResolveProperty failed - Couldn't find {name} in {t.Name}");
			return null;
		}

		private static void SetupUnityTypes()
		{
			vector2Type = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Vector2");
			if (vector2Type == null)
			{
				Log.Error("Vector2Type is null!");
			}
			vector3Type = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Vector3");
			vector4Type = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Vector4");
			colorType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Color");
			color32Type = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Color32");
			quaternionType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Quaternion");
			rectType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Rect");
			planeType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Plane");
			rayType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Ray");
			matrixType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Matrix4x4");
			gameObjectType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.GameObject");
			if (gameObjectType == null)
			{
				Log.Error("GameObjectType is null!");
			}
			transformType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Transform");
			unityObjectType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Object");

			hashType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkHash128");

			NetworkClientType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkClient");
			NetworkServerType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkServer");
			NetworkCRCType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkCRC");

			SyncVarType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncVarAttribute");

			CommandType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.CommandAttribute");

			ClientRpcType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.ClientRpcAttribute");

			TargetRpcType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.TargetRpcAttribute");

			SyncEventType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncEventAttribute");

			SyncListType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncList`1");

			NetworkSettingsType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkSettingsAttribute");

			SyncListFloatType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListFloat");

			SyncListIntType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListInt");

			SyncListUIntType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListUInt");

			SyncListBoolType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListBool");

			SyncListStringType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListString");

		}

		private static void SetupCorLib()
		{
			var name = AssemblyNameReference.Parse("mscorlib");
			var parameters = new ReaderParameters
			{
				AssemblyResolver = scriptDef.MainModule.AssemblyResolver
			};
			corLib = scriptDef.MainModule.AssemblyResolver.Resolve(name, parameters).MainModule;
		}

		private static TypeReference ImportCorLibType(string fullName)
		{
			var type = corLib.GetType(fullName) ?? Enumerable.First<ExportedType>(corLib.ExportedTypes, (ExportedType t) => t.FullName == fullName).Resolve();
			return scriptDef.MainModule.ImportReference(type);
		}

		private static void SetupTargetTypes()
		{
			SetupCorLib();
			voidType = ImportCorLibType("System.Void");
			singleType = ImportCorLibType("System.Single");
			doubleType = ImportCorLibType("System.Double");
			decimalType = ImportCorLibType("System.Decimal");
			boolType = ImportCorLibType("System.Boolean");
			stringType = ImportCorLibType("System.String");
			int64Type = ImportCorLibType("System.Int64");
			uint64Type = ImportCorLibType("System.UInt64");
			int32Type = ImportCorLibType("System.Int32");
			uint32Type = ImportCorLibType("System.UInt32");
			int16Type = ImportCorLibType("System.Int16");
			uint16Type = ImportCorLibType("System.UInt16");
			byteType = ImportCorLibType("System.Byte");
			sbyteType = ImportCorLibType("System.SByte");
			charType = ImportCorLibType("System.Char");
			objectType = ImportCorLibType("System.Object");
			valueTypeType = ImportCorLibType("System.ValueType");
			typeType = ImportCorLibType("System.Type");
			IEnumeratorType = ImportCorLibType("System.Collections.IEnumerator");
			MemoryStreamType = ImportCorLibType("System.IO.MemoryStream");

			NetworkReaderType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.Transport.QNetworkReader");

			NetworkReaderDef = NetworkReaderType.Resolve();
			NetworkReaderCtor = ResolveMethod(NetworkReaderDef, ".ctor");

			NetworkWriterType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.Transport.QNetworkWriter");

			NetworkWriterDef = NetworkWriterType.Resolve();
			NetworkWriterCtor = ResolveMethod(NetworkWriterDef, ".ctor");
			MemoryStreamCtor = ResolveMethod(MemoryStreamType, ".ctor");

			NetworkInstanceIdType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkInstanceId");
			NetworkSceneIdType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkSceneId");
			NetworkInstanceIdType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkInstanceId");
			NetworkSceneIdType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkSceneId");

			NetworkServerGetActive = ResolveMethod(NetworkServerType, "get_active");
			NetworkServerGetLocalClientActive = ResolveMethod(NetworkServerType, "get_localClientActive");
			NetworkClientGetActive = ResolveMethod(NetworkClientType, "get_active");
			NetworkReaderReadInt32 = ResolveMethod(NetworkReaderType, "ReadInt32");
			NetworkWriterWriteInt32 = ResolveMethodWithArg(NetworkWriterType, "Write", int32Type);
			NetworkWriterWriteInt16 = ResolveMethodWithArg(NetworkWriterType, "Write", int16Type);
			NetworkReaderReadPacked32 = ResolveMethod(NetworkReaderType, "ReadPackedUInt32");
			NetworkReaderReadPacked64 = ResolveMethod(NetworkReaderType, "ReadPackedUInt64");
			NetworkReaderReadByte = ResolveMethod(NetworkReaderType, "ReadByte");
			NetworkWriterWritePacked32 = ResolveMethod(NetworkWriterType, "WritePackedUInt32");
			NetworkWriterWritePacked64 = ResolveMethod(NetworkWriterType, "WritePackedUInt64");
			NetworkWriterWriteNetworkInstanceId = ResolveMethodWithArg(NetworkWriterType, "Write", NetworkInstanceIdType);
			NetworkWriterWriteNetworkSceneId = ResolveMethodWithArg(NetworkWriterType, "Write", NetworkSceneIdType);
			NetworkReaderReadNetworkInstanceId = ResolveMethod(NetworkReaderType, "ReadNetworkId");
			NetworkReaderReadNetworkSceneId = ResolveMethod(NetworkReaderType, "ReadSceneId");
			NetworkInstanceIsEmpty = ResolveMethod(NetworkInstanceIdType, "IsEmpty");
			NetworkReadUInt16 = ResolveMethod(NetworkReaderType, "ReadUInt16");
			NetworkWriteUInt16 = ResolveMethodWithArg(NetworkWriterType, "Write", uint16Type);

			CmdDelegateReference = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkBehaviour/CmdDelegate");

			CmdDelegateConstructor = ResolveMethod(CmdDelegateReference, ".ctor");
			Console.WriteLine("gameobject");
			scriptDef.MainModule.ImportReference(gameObjectType);
			Console.WriteLine("transform");
			scriptDef.MainModule.ImportReference(transformType);

			TypeReference type = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.Components.QNetworkIdentity");

			Console.WriteLine("type");
			NetworkIdentityType = scriptDef.MainModule.ImportReference(type);
			Console.WriteLine("networkinstanceidtype");
			NetworkInstanceIdType = scriptDef.MainModule.ImportReference(NetworkInstanceIdType);
			SyncListFloatReadType = ResolveMethod(SyncListFloatType, "ReadReference");
			SyncListIntReadType = ResolveMethod(SyncListIntType, "ReadReference");
			SyncListUIntReadType = ResolveMethod(SyncListUIntType, "ReadReference");
			SyncListBoolReadType = ResolveMethod(SyncListBoolType, "ReadReference");
			SyncListStringReadType = ResolveMethod(SyncListStringType, "ReadReference");
			SyncListFloatWriteType = ResolveMethod(SyncListFloatType, "WriteInstance");
			SyncListIntWriteType = ResolveMethod(SyncListIntType, "WriteInstance");
			SyncListUIntWriteType = ResolveMethod(SyncListUIntType, "WriteInstance");
			SyncListBoolWriteType = ResolveMethod(SyncListBoolType, "WriteInstance");
			SyncListStringWriteType = ResolveMethod(SyncListStringType, "WriteInstance");

			NetworkBehaviourType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkBehaviour");

			Console.WriteLine("networkbehaviourtype");
			NetworkBehaviourType2 = scriptDef.MainModule.ImportReference(NetworkBehaviourType);

			NetworkConnectionType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkConnection");

			MonoBehaviourType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.MonoBehaviour");
			ScriptableObjectType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.ScriptableObject");

			NetworkConnectionType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkConnection");

			NetworkConnectionType = scriptDef.MainModule.ImportReference(NetworkConnectionType);

			ULocalConnectionToServerType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QULocalConnectionToServer");

			ULocalConnectionToServerType = scriptDef.MainModule.ImportReference(ULocalConnectionToServerType);

			ULocalConnectionToClientType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QULocalConnectionToClient");

			ULocalConnectionToClientType = scriptDef.MainModule.ImportReference(ULocalConnectionToClientType);

			MessageBaseType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.Messages.QMessageBase");
			SyncListStructType = UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListStruct`1");

			NetworkBehaviourDirtyBitsReference = ResolveProperty(NetworkBehaviourType, "SyncVarDirtyBits");
			ComponentType = UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Component");

			ClientSceneType = QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QClientScene");

			FindLocalObjectReference = ResolveMethod(ClientSceneType, "FindLocalObject");
			RegisterBehaviourReference = ResolveMethod(NetworkCRCType, "RegisterBehaviour");
			ReadyConnectionReference = ResolveMethod(ClientSceneType, "get_readyConnection");
			getComponentReference = ResolveMethodGeneric(ComponentType, "GetComponent", NetworkIdentityType);
			getUNetIdReference = ResolveMethod(type, "get_NetId");
			gameObjectInequality = ResolveMethod(unityObjectType, "op_Inequality");
			UBehaviourIsServer = ResolveMethod(NetworkBehaviourType, "get_IsServer");
			getPlayerIdReference = ResolveMethod(NetworkBehaviourType, "get_PlayerControllerId");
			setSyncVarReference = ResolveMethod(NetworkBehaviourType, "SetSyncVar");
			setSyncVarHookGuard = ResolveMethod(NetworkBehaviourType, "set_SyncVarHookGuard");
			getSyncVarHookGuard = ResolveMethod(NetworkBehaviourType, "get_SyncVarHookGuard");
			setSyncVarGameObjectReference = ResolveMethod(NetworkBehaviourType, "SetSyncVarGameObject");
			registerCommandDelegateReference = ResolveMethod(NetworkBehaviourType, "RegisterCommandDelegate");
			registerRpcDelegateReference = ResolveMethod(NetworkBehaviourType, "RegisterRpcDelegate");
			registerEventDelegateReference = ResolveMethod(NetworkBehaviourType, "RegisterEventDelegate");
			registerSyncListDelegateReference = ResolveMethod(NetworkBehaviourType, "RegisterSyncListDelegate");
			getTypeReference = ResolveMethod(objectType, "GetType");
			getTypeFromHandleReference = ResolveMethod(typeType, "GetTypeFromHandle");
			logErrorReference = ResolveMethod(UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Debug"), "LogError");
			logWarningReference = ResolveMethod(UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Debug"), "LogWarning");
			sendCommandInternal = ResolveMethod(NetworkBehaviourType, "SendCommandInternal");
			sendRpcInternal = ResolveMethod(NetworkBehaviourType, "SendRPCInternal");
			sendTargetRpcInternal = ResolveMethod(NetworkBehaviourType, "SendTargetRPCInternal");
			sendEventInternal = ResolveMethod(NetworkBehaviourType, "SendEventInternal");
			Console.WriteLine("synclisttype");
			SyncListType = scriptDef.MainModule.ImportReference(SyncListType);
			SyncListInitBehaviourReference = ResolveMethod(SyncListType, "InitializeBehaviour");
			SyncListInitHandleMsg = ResolveMethod(SyncListType, "HandleMsg");
			SyncListClear = ResolveMethod(SyncListType, "Clear");
		}

		private static void SetupReadFunctions()
		{
			var weaverLists = lists;
			var dictionary = new Dictionary<string, MethodReference>
			{
				{ singleType.FullName, ResolveMethod(NetworkReaderType, "ReadSingle") },
				{ doubleType.FullName, ResolveMethod(NetworkReaderType, "ReadDouble") },
				{ boolType.FullName, ResolveMethod(NetworkReaderType, "ReadBoolean") },
				{ stringType.FullName, ResolveMethod(NetworkReaderType, "ReadString") },
				{ int64Type.FullName, NetworkReaderReadPacked64 },
				{ uint64Type.FullName, NetworkReaderReadPacked64 },
				{ int32Type.FullName, NetworkReaderReadPacked32 },
				{ uint32Type.FullName, NetworkReaderReadPacked32 },
				{ int16Type.FullName, NetworkReaderReadPacked32 },
				{ uint16Type.FullName, NetworkReaderReadPacked32 },
				{ byteType.FullName, NetworkReaderReadPacked32 },
				{ sbyteType.FullName, NetworkReaderReadPacked32 },
				{ charType.FullName, NetworkReaderReadPacked32 },
				{ decimalType.FullName, ResolveMethod(NetworkReaderType, "ReadDecimal") },
				{ vector2Type.FullName, ResolveMethod(NetworkReaderType, "ReadVector2") },
				{ vector3Type.FullName, ResolveMethod(NetworkReaderType, "ReadVector3") },
				{ vector4Type.FullName, ResolveMethod(NetworkReaderType, "ReadVector4") },
				{ colorType.FullName, ResolveMethod(NetworkReaderType, "ReadColor") },
				{ color32Type.FullName, ResolveMethod(NetworkReaderType, "ReadColor32") },
				{ quaternionType.FullName, ResolveMethod(NetworkReaderType, "ReadQuaternion") },
				{ rectType.FullName, ResolveMethod(NetworkReaderType, "ReadRect") },
				{ planeType.FullName, ResolveMethod(NetworkReaderType, "ReadPlane") },
				{ rayType.FullName, ResolveMethod(NetworkReaderType, "ReadRay") },
				{ matrixType.FullName, ResolveMethod(NetworkReaderType, "ReadMatrix4x4") },
				{ hashType.FullName, ResolveMethod(NetworkReaderType, "ReadNetworkHash128") },
				{ gameObjectType.FullName, ResolveMethod(NetworkReaderType, "ReadGameObject") },
				{ NetworkIdentityType.FullName, ResolveMethod(NetworkReaderType, "ReadNetworkIdentity") },
				{ NetworkInstanceIdType.FullName, NetworkReaderReadNetworkInstanceId },
				{ NetworkSceneIdType.FullName, NetworkReaderReadNetworkSceneId },
				{ transformType.FullName, ResolveMethod(NetworkReaderType, "ReadTransform") },
				{ "System.Byte[]", ResolveMethod(NetworkReaderType, "ReadBytesAndSize") }
			};
			weaverLists.readFuncs = dictionary;
			var weaverLists2 = lists;
			dictionary = new Dictionary<string, MethodReference>
			{
				{ SyncListFloatType.FullName, SyncListFloatReadType },
				{ SyncListIntType.FullName, SyncListIntReadType },
				{ SyncListUIntType.FullName, SyncListUIntReadType },
				{ SyncListBoolType.FullName, SyncListBoolReadType },
				{ SyncListStringType.FullName, SyncListStringReadType }
			};
			weaverLists2.readByReferenceFuncs = dictionary;
		}

		private static void SetupWriteFunctions()
		{
			var weaverLists = lists;
			var dictionary = new Dictionary<string, MethodReference>
			{
				{ singleType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", singleType) },
				{ doubleType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", doubleType) },
				{ boolType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", boolType) },
				{ stringType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", stringType) },
				{ int64Type.FullName, NetworkWriterWritePacked64 },
				{ uint64Type.FullName, NetworkWriterWritePacked64 },
				{ int32Type.FullName, NetworkWriterWritePacked32 },
				{ uint32Type.FullName, NetworkWriterWritePacked32 },
				{ int16Type.FullName, NetworkWriterWritePacked32 },
				{ uint16Type.FullName, NetworkWriterWritePacked32 },
				{ byteType.FullName, NetworkWriterWritePacked32 },
				{ sbyteType.FullName, NetworkWriterWritePacked32 },
				{ charType.FullName, NetworkWriterWritePacked32 },
				{ decimalType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", decimalType) },
				{ vector2Type.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", vector2Type) },
				{ vector3Type.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", vector3Type) },
				{ vector4Type.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", vector4Type) },
				{ colorType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", colorType) },
				{ color32Type.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", color32Type) },
				{ quaternionType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", quaternionType) },
				{ rectType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", rectType) },
				{ planeType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", planeType) },
				{ rayType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", rayType) },
				{ matrixType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", matrixType) },
				{ hashType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", hashType) },
				{ gameObjectType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", gameObjectType) },
				{ NetworkIdentityType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", NetworkIdentityType) },
				{ NetworkInstanceIdType.FullName, NetworkWriterWriteNetworkInstanceId },
				{ NetworkSceneIdType.FullName, NetworkWriterWriteNetworkSceneId },
				{ transformType.FullName, ResolveMethodWithArg(NetworkWriterType, "Write", transformType) },
				{ "System.Byte[]", ResolveMethod(NetworkWriterType, "WriteBytesFull") },
				{ SyncListFloatType.FullName, SyncListFloatWriteType },
				{ SyncListIntType.FullName, SyncListIntWriteType },
				{ SyncListUIntType.FullName, SyncListUIntWriteType },
				{ SyncListBoolType.FullName, SyncListBoolWriteType },
				{ SyncListStringType.FullName, SyncListStringWriteType }
			};
			weaverLists.writeFuncs = dictionary;
		}

		private static bool IsNetworkBehaviour(TypeDefinition td)
		{
			bool result;
			if (!td.IsClass)
			{
				result = false;
			}
			else
			{
				var baseType = td.BaseType;
				while (baseType != null)
				{
					if (baseType.FullName == NetworkBehaviourType.FullName)
					{
						return true;
					}
					try
					{
						baseType = baseType.Resolve().BaseType;
					}
					catch (AssemblyResolutionException)
					{
						break;
					}
				}
				result = false;
			}
			return result;
		}

		public static bool IsDerivedFrom(TypeDefinition td, TypeReference baseClass)
		{
			bool result;
			if (!td.IsClass)
			{
				result = false;
			}
			else
			{
				var baseType = td.BaseType;
				while (baseType != null)
				{
					var text = baseType.FullName;
					var num = text.IndexOf('<');
					if (num != -1)
					{
						text = text.Substring(0, num);
					}
					if (text == baseClass.FullName)
					{
						return true;
					}
					try
					{
						baseType = baseType.Resolve().BaseType;
					}
					catch (AssemblyResolutionException)
					{
						break;
					}
				}
				result = false;
			}
			return result;
		}

		public static bool IsValidTypeToGenerate(TypeDefinition variable)
		{
			var name = scriptDef.MainModule.Name;
			bool result;
			if (variable.Module.Name != name)
			{
				Log.Error(string.Concat(new string[]
				{
					"parameter [",
					variable.Name,
					"] is of the type [",
					variable.FullName,
					"] is not a valid type, please make sure to use a valid type."
				}));
				fail = true;
				fail = true;
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		private static void CheckMonoBehaviour(TypeDefinition td)
		{
			if (IsDerivedFrom(td, MonoBehaviourType))
			{
				ProcessMonoBehaviourType(td);
			}
		}

		private static bool CheckNetworkBehaviour(TypeDefinition td)
		{
			bool result;
			if (!td.IsClass)
			{
				result = false;
			}
			else if (!IsNetworkBehaviour(td))
			{
				CheckMonoBehaviour(td);
				result = false;
			}
			else
			{
				var list = new List<TypeDefinition>();
				var typeDefinition = td;
				while (typeDefinition != null)
				{
					if (typeDefinition.FullName == NetworkBehaviourType.FullName)
					{
						break;
					}
					try
					{
						list.Insert(0, typeDefinition);
						typeDefinition = typeDefinition.BaseType.Resolve();
					}
					catch (AssemblyResolutionException)
					{
						break;
					}
				}
				var flag = false;
				foreach (var td2 in list)
				{
					flag |= ProcessNetworkBehaviourType(td2);
				}
				result = flag;
			}
			return result;
		}

		private static bool CheckMessageBase(TypeDefinition td)
		{
			bool result;
			if (!td.IsClass)
			{
				result = false;
			}
			else
			{
				var flag = false;
				var baseType = td.BaseType;
				while (baseType != null)
				{
					if (baseType.FullName == MessageBaseType.FullName)
					{
						flag |= ProcessMessageType(td);
						break;
					}
					try
					{
						baseType = baseType.Resolve().BaseType;
					}
					catch (AssemblyResolutionException)
					{
						break;
					}
				}
				foreach (var td2 in td.NestedTypes)
				{
					flag |= CheckMessageBase(td2);
				}
				result = flag;
			}
			return result;
		}

		private static bool CheckSyncListStruct(TypeDefinition td)
		{
			bool result;
			if (!td.IsClass)
			{
				result = false;
			}
			else
			{
				var flag = false;
				var baseType = td.BaseType;
				while (baseType != null)
				{
					if (baseType.FullName.Contains("SyncListStruct"))
					{
						flag |= ProcessSyncListStructType(td);
						break;
					}
					try
					{
						baseType = baseType.Resolve().BaseType;
					}
					catch (AssemblyResolutionException)
					{
						break;
					}
				}
				foreach (var td2 in td.NestedTypes)
				{
					flag |= CheckSyncListStruct(td2);
				}
				result = flag;
			}
			return result;
		}

		private static bool Weave(string assName, IEnumerable<string> dependencies, IAssemblyResolver assemblyResolver, string unityEngineDLLPath, string unityUNetDLLPath, string outputDir)
		{
			var readerParameters = Helpers.ReaderParameters(assName, dependencies, assemblyResolver, unityEngineDLLPath, unityUNetDLLPath);
			scriptDef = AssemblyDefinition.ReadAssembly(assName, readerParameters);
			SetupTargetTypes();
			SetupReadFunctions();
			SetupWriteFunctions();
			var mainModule = scriptDef.MainModule;
			Console.WriteLine("Script Module: {0}", mainModule.Name);
			var flag = false;
			for (var i = 0; i < 2; i++)
			{
				var stopwatch = Stopwatch.StartNew();
				foreach (var typeDefinition in mainModule.Types)
				{
					if (typeDefinition.IsClass && CanBeResolved(typeDefinition.BaseType))
					{
						try
						{
							if (i == 0)
							{
								flag |= CheckSyncListStruct(typeDefinition);
							}
							else
							{
								flag |= CheckNetworkBehaviour(typeDefinition);
								flag |= CheckMessageBase(typeDefinition);
							}
						}
						catch (Exception ex)
						{
							if (scriptDef.MainModule.SymbolReader != null)
							{
								scriptDef.MainModule.SymbolReader.Dispose();
							}
							fail = true;
							throw ex;
						}
					}
					if (fail)
					{
						if (scriptDef.MainModule.SymbolReader != null)
						{
							scriptDef.MainModule.SymbolReader.Dispose();
						}
						return false;
					}
				}
				stopwatch.Stop();
				Console.WriteLine(string.Concat(new object[]
				{
					"Pass: ",
					i,
					" took ",
					stopwatch.ElapsedMilliseconds,
					" milliseconds"
				}));
			}
			if (flag)
			{
				foreach (var methodDefinition in lists.replacedMethods)
				{
					lists.replacementMethodNames.Add(methodDefinition.FullName);
				}
				try
				{
					ProcessPropertySites();
				}
				catch (Exception ex2)
				{
					Log.Error("ProcessPropertySites exception: " + ex2);
					if (scriptDef.MainModule.SymbolReader != null)
					{
						scriptDef.MainModule.SymbolReader.Dispose();
					}
					return false;
				}
				if (fail)
				{
					if (scriptDef.MainModule.SymbolReader != null)
					{
						scriptDef.MainModule.SymbolReader.Dispose();
					}
					return false;
				}
				var fileName = Helpers.DestinationFileFor(outputDir, assName);
				var writerParameters = Helpers.GetWriterParameters(readerParameters);
				if (writerParameters.SymbolWriterProvider is PdbWriterProvider)
				{
					writerParameters.SymbolWriterProvider = new MdbWriterProvider();
					var text = Path.ChangeExtension(assName, ".pdb");
					File.Delete(text);
				}
				scriptDef.Write(fileName, writerParameters);
			}
			if (scriptDef.MainModule.SymbolReader != null)
			{
				scriptDef.MainModule.SymbolReader.Dispose();
			}
			return true;
		}

		public static bool WeaveAssemblies(string assembly, IEnumerable<string> dependencies, IAssemblyResolver assemblyResolver, string outputDir, string unityEngineDLLPath, string unityQNetDLLPath, string unityUNetDLLPath)
		{
			fail = false;
			lists = new WeaverLists();
			Console.WriteLine($"load unity engine from {unityEngineDLLPath}");
			UnityAssemblyDefinition = AssemblyDefinition.ReadAssembly(unityEngineDLLPath);
			Console.WriteLine($"load qnet from {unityQNetDLLPath}");
			QNetAssemblyDefinition = AssemblyDefinition.ReadAssembly(unityQNetDLLPath);
			Console.WriteLine($"load unet from {unityUNetDLLPath}");
			UNetAssemblyDefinition = AssemblyDefinition.ReadAssembly(unityUNetDLLPath);
			SetupUnityTypes();
			try
			{
				if (!Weave(assembly, dependencies, assemblyResolver, unityEngineDLLPath, unityQNetDLLPath, outputDir))
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception :" + ex);
				return false;
			}
			corLib = null;
			return true;
		}

		public static TypeReference NetworkBehaviourType;

		public static TypeReference NetworkBehaviourType2;

		public static TypeReference MonoBehaviourType;

		public static TypeReference ScriptableObjectType;

		public static TypeReference NetworkConnectionType;

		public static TypeReference ULocalConnectionToServerType;

		public static TypeReference ULocalConnectionToClientType;

		public static TypeReference MessageBaseType;

		public static TypeReference SyncListStructType;

		public static MethodReference NetworkBehaviourDirtyBitsReference;

		public static TypeReference NetworkClientType;

		public static TypeReference NetworkServerType;

		public static TypeReference NetworkCRCType;

		public static TypeReference NetworkReaderType;

		public static TypeDefinition NetworkReaderDef;

		public static TypeReference NetworkWriterType;

		public static TypeDefinition NetworkWriterDef;

		public static MethodReference NetworkWriterCtor;

		public static MethodReference NetworkReaderCtor;

		public static TypeReference MemoryStreamType;

		public static MethodReference MemoryStreamCtor;

		public static MethodReference getComponentReference;

		public static MethodReference getUNetIdReference;

		public static MethodReference getPlayerIdReference;

		public static TypeReference NetworkIdentityType;

		public static TypeReference NetworkInstanceIdType;

		public static TypeReference NetworkSceneIdType;

		public static TypeReference IEnumeratorType;

		public static TypeReference ClientSceneType;

		public static MethodReference FindLocalObjectReference;

		public static MethodReference RegisterBehaviourReference;

		public static MethodReference ReadyConnectionReference;

		public static TypeReference ComponentType;

		public static TypeReference CmdDelegateReference;

		public static MethodReference CmdDelegateConstructor;

		public static MethodReference NetworkReaderReadInt32;

		public static MethodReference NetworkWriterWriteInt32;

		public static MethodReference NetworkWriterWriteInt16;

		public static MethodReference NetworkServerGetActive;

		public static MethodReference NetworkServerGetLocalClientActive;

		public static MethodReference NetworkClientGetActive;

		public static MethodReference UBehaviourIsServer;

		public static MethodReference NetworkReaderReadPacked32;

		public static MethodReference NetworkReaderReadPacked64;

		public static MethodReference NetworkReaderReadByte;

		public static MethodReference NetworkWriterWritePacked32;

		public static MethodReference NetworkWriterWritePacked64;

		public static MethodReference NetworkWriterWriteNetworkInstanceId;

		public static MethodReference NetworkWriterWriteNetworkSceneId;

		public static MethodReference NetworkReaderReadNetworkInstanceId;

		public static MethodReference NetworkReaderReadNetworkSceneId;

		public static MethodReference NetworkInstanceIsEmpty;

		public static MethodReference NetworkReadUInt16;

		public static MethodReference NetworkWriteUInt16;

		public static TypeReference SyncVarType;

		public static TypeReference CommandType;

		public static TypeReference ClientRpcType;

		public static TypeReference TargetRpcType;

		public static TypeReference SyncEventType;

		public static TypeReference SyncListType;

		public static MethodReference SyncListInitBehaviourReference;

		public static MethodReference SyncListInitHandleMsg;

		public static MethodReference SyncListClear;

		public static TypeReference NetworkSettingsType;

		public static TypeReference SyncListFloatType;

		public static TypeReference SyncListIntType;

		public static TypeReference SyncListUIntType;

		public static TypeReference SyncListBoolType;

		public static TypeReference SyncListStringType;

		public static MethodReference SyncListFloatReadType;

		public static MethodReference SyncListIntReadType;

		public static MethodReference SyncListUIntReadType;

		public static MethodReference SyncListStringReadType;

		public static MethodReference SyncListBoolReadType;

		public static MethodReference SyncListFloatWriteType;

		public static MethodReference SyncListIntWriteType;

		public static MethodReference SyncListUIntWriteType;

		public static MethodReference SyncListBoolWriteType;

		public static MethodReference SyncListStringWriteType;

		public static TypeReference voidType;

		public static TypeReference singleType;

		public static TypeReference doubleType;

		public static TypeReference decimalType;

		public static TypeReference boolType;

		public static TypeReference stringType;

		public static TypeReference int64Type;

		public static TypeReference uint64Type;

		public static TypeReference int32Type;

		public static TypeReference uint32Type;

		public static TypeReference int16Type;

		public static TypeReference uint16Type;

		public static TypeReference byteType;

		public static TypeReference sbyteType;

		public static TypeReference charType;

		public static TypeReference objectType;

		public static TypeReference valueTypeType;

		public static TypeReference vector2Type;

		public static TypeReference vector3Type;

		public static TypeReference vector4Type;

		public static TypeReference colorType;

		public static TypeReference color32Type;

		public static TypeReference quaternionType;

		public static TypeReference rectType;

		public static TypeReference rayType;

		public static TypeReference planeType;

		public static TypeReference matrixType;

		public static TypeReference hashType;

		public static TypeReference typeType;

		public static TypeReference gameObjectType;

		public static TypeReference transformType;

		public static TypeReference unityObjectType;

		public static MethodReference gameObjectInequality;

		public static MethodReference setSyncVarReference;

		public static MethodReference setSyncVarHookGuard;

		public static MethodReference getSyncVarHookGuard;

		public static MethodReference setSyncVarGameObjectReference;

		public static MethodReference registerCommandDelegateReference;

		public static MethodReference registerRpcDelegateReference;

		public static MethodReference registerEventDelegateReference;

		public static MethodReference registerSyncListDelegateReference;

		public static MethodReference getTypeReference;

		public static MethodReference getTypeFromHandleReference;

		public static MethodReference logErrorReference;

		public static MethodReference logWarningReference;

		public static MethodReference sendCommandInternal;

		public static MethodReference sendRpcInternal;

		public static MethodReference sendTargetRpcInternal;

		public static MethodReference sendEventInternal;

		public static WeaverLists lists;

		public static AssemblyDefinition scriptDef;

		public static ModuleDefinition corLib;

		public static AssemblyDefinition UnityAssemblyDefinition;

		public static AssemblyDefinition QNetAssemblyDefinition;

		public static AssemblyDefinition UNetAssemblyDefinition;

		private static bool m_DebugFlag = true;

		public static bool fail;

		public static bool generateLogErrors = false;

		private const int MaxRecursionCount = 128;

		private static int s_RecursionCount;
	}
}
