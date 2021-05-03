using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace QNetWeaver
{
	internal class Weaver
	{
		public static void ResetRecursionCount()
		{
			Weaver.s_RecursionCount = 0;
		}

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

		public static bool IsArrayType(TypeReference variable)
		{
			return (!variable.IsArray || !((ArrayType)variable).ElementType.IsArray) && (!variable.IsArray || ((ArrayType)variable).Rank <= 1);
		}

		public static void DLog(TypeDefinition td, string fmt, params object[] args)
		{
			if (Weaver.m_DebugFlag)
			{
				Console.WriteLine("[" + td.Name + "] " + string.Format(fmt, args));
			}
		}

		public static int GetSyncVarStart(string className)
		{
			int result;
			if (Weaver.lists.numSyncVars.ContainsKey(className))
			{
				var num = Weaver.lists.numSyncVars[className];
				result = num;
			}
			else
			{
				result = 0;
			}
			return result;
		}

		public static void SetNumSyncVars(string className, int num)
		{
			Weaver.lists.numSyncVars[className] = num;
		}

		public static MethodReference GetWriteFunc(TypeReference variable)
		{
			MethodReference result;
			if (Weaver.s_RecursionCount++ > 128)
			{
				Log.Error("GetWriteFunc recursion depth exceeded for " + variable.Name + ". Check for self-referencing member variables.");
				Weaver.fail = true;
				result = null;
			}
			else
			{
				if (Weaver.lists.writeFuncs.ContainsKey(variable.FullName))
				{
					var methodReference = Weaver.lists.writeFuncs[variable.FullName];
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
						var writeFunc = Weaver.GetWriteFunc(elementType);
						if (writeFunc == null)
						{
							return null;
						}
						methodDefinition = Weaver.GenerateArrayWriteFunc(variable, writeFunc);
					}
					else
					{
						if (variable.Resolve().IsEnum)
						{
							return Weaver.NetworkWriterWriteInt32;
						}
						methodDefinition = Weaver.GenerateWriterFunction(variable);
					}
					if (methodDefinition == null)
					{
						result = null;
					}
					else
					{
						Weaver.RegisterWriteFunc(variable.FullName, methodDefinition);
						result = methodDefinition;
					}
				}
			}
			return result;
		}

		public static void RegisterWriteFunc(string name, MethodDefinition newWriterFunc)
		{
			Weaver.lists.writeFuncs[name] = newWriterFunc;
			Weaver.lists.generatedWriteFunctions.Add(newWriterFunc);
			Weaver.ConfirmGeneratedCodeClass(Weaver.scriptDef.MainModule);
			Weaver.lists.generateContainerClass.Methods.Add(newWriterFunc);
		}

		public static MethodReference GetReadByReferenceFunc(TypeReference variable)
		{
			MethodReference result;
			if (Weaver.lists.readByReferenceFuncs.ContainsKey(variable.FullName))
			{
				result = Weaver.lists.readByReferenceFuncs[variable.FullName];
			}
			else
			{
				result = null;
			}
			return result;
		}

		public static MethodReference GetReadFunc(TypeReference variable)
		{
			if (Weaver.lists.readFuncs.ContainsKey(variable.FullName))
			{
				var methodReference = Weaver.lists.readFuncs[variable.FullName];
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
					var readFunc = Weaver.GetReadFunc(elementType);
					if (readFunc == null)
					{
						return null;
					}
					methodDefinition = Weaver.GenerateArrayReadFunc(variable, readFunc);
				}
				else
				{
					if (typeDefinition.IsEnum)
					{
						return Weaver.NetworkReaderReadInt32;
					}
					methodDefinition = Weaver.GenerateReadFunction(variable);
				}
				if (methodDefinition == null)
				{
					Log.Error("GetReadFunc unable to generate function for:" + variable.FullName);
					result = null;
				}
				else
				{
					Weaver.RegisterReadFunc(variable.FullName, methodDefinition);
					result = methodDefinition;
				}
			}
			return result;
		}

		public static void RegisterReadByReferenceFunc(string name, MethodDefinition newReaderFunc)
		{
			Weaver.lists.readByReferenceFuncs[name] = newReaderFunc;
			Weaver.lists.generatedReadFunctions.Add(newReaderFunc);
			Weaver.ConfirmGeneratedCodeClass(Weaver.scriptDef.MainModule);
			Weaver.lists.generateContainerClass.Methods.Add(newReaderFunc);
		}

		public static void RegisterReadFunc(string name, MethodDefinition newReaderFunc)
		{
			Weaver.lists.readFuncs[name] = newReaderFunc;
			Weaver.lists.generatedReadFunctions.Add(newReaderFunc);
			Weaver.ConfirmGeneratedCodeClass(Weaver.scriptDef.MainModule);
			Weaver.lists.generateContainerClass.Methods.Add(newReaderFunc);
		}

		private static MethodDefinition GenerateArrayReadFunc(TypeReference variable, MethodReference elementReadFunc)
		{
			MethodDefinition result;
			if (!Weaver.IsArrayType(variable))
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
				methodDefinition.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkReaderType)));
				methodDefinition.Body.Variables.Add(new VariableDefinition(Weaver.int32Type));
				methodDefinition.Body.Variables.Add(new VariableDefinition(variable));
				methodDefinition.Body.Variables.Add(new VariableDefinition(Weaver.int32Type));
				methodDefinition.Body.InitLocals = true;
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.NetworkReadUInt16));
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
			if (!Weaver.IsArrayType(variable))
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
				var methodDefinition = new MethodDefinition(text, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
				methodDefinition.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkWriterType)));
				methodDefinition.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(variable)));
				methodDefinition.Body.Variables.Add(new VariableDefinition(Weaver.uint16Type));
				methodDefinition.Body.Variables.Add(new VariableDefinition(Weaver.uint16Type));
				methodDefinition.Body.InitLocals = true;
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				var instruction = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brtrue, instruction));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.NetworkWriteUInt16));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				ilprocessor.Append(instruction);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldlen));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Conv_I4));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Conv_U2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.NetworkWriteUInt16));
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
			if (!Weaver.IsValidTypeToGenerate(variable.Resolve()))
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
				var methodDefinition = new MethodDefinition(text, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
				methodDefinition.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkWriterType)));
				methodDefinition.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(variable)));
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				var num = 0U;
				foreach (var fieldDefinition in variable.Resolve().Fields)
				{
					if (!fieldDefinition.IsStatic && !fieldDefinition.IsPrivate)
					{
						if (fieldDefinition.FieldType.Resolve().HasGenericParameters)
						{
							Weaver.fail = true;
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
							Weaver.fail = true;
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
						var writeFunc = Weaver.GetWriteFunc(fieldDefinition.FieldType);
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
							Weaver.fail = true;
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
			if (!Weaver.IsValidTypeToGenerate(variable.Resolve()))
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
				methodDefinition.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkReaderType)));
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				if (variable.IsValueType)
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloca, 0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Initobj, variable));
				}
				else
				{
					var methodDefinition2 = Weaver.ResolveDefaultPublicCtor(variable);
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
						var readFunc = Weaver.GetReadFunc(fieldDefinition.FieldType);
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
							Weaver.fail = true;
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
						Weaver.DLog(td, "    " + instruction.Operand, new object[0]);
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
						for (var i = 0; i < Weaver.lists.replacedEvents.Count; i++)
						{
							var eventDefinition = Weaver.lists.replacedEvents[i];
							if (eventDefinition.Name == fieldReference.Name)
							{
								instr.Operand = Weaver.lists.replacementEvents[i];
								instruction.OpCode = OpCodes.Nop;
								flag = true;
								break;
							}
						}
					}
				}
			}
			else if (Weaver.lists.replacementMethodNames.Contains(opMethodRef.FullName))
			{
				for (var j = 0; j < Weaver.lists.replacedMethods.Count; j++)
				{
					var methodDefinition = Weaver.lists.replacedMethods[j];
					if (opMethodRef.FullName == methodDefinition.FullName)
					{
						instr.Operand = Weaver.lists.replacementMethods[j];
						break;
					}
				}
			}
		}

		private static void ConfirmGeneratedCodeClass(ModuleDefinition moduleDef)
		{
			if (Weaver.lists.generateContainerClass == null)
			{
				Weaver.lists.generateContainerClass = new TypeDefinition("Unity", "GeneratedNetworkCode", TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.BeforeFieldInit, Weaver.objectType);
				var methodDefinition = new MethodDefinition(".ctor", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, Weaver.voidType);
				methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
				methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Call, Weaver.ResolveMethod(Weaver.objectType, ".ctor")));
				methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
				Weaver.lists.generateContainerClass.Methods.Add(methodDefinition);
			}
		}

		private static void ProcessInstructionField(TypeDefinition td, MethodDefinition md, Instruction i, FieldDefinition opField)
		{
			if (!(md.Name == ".ctor") && !(md.Name == "OnDeserialize"))
			{
				for (var j = 0; j < Weaver.lists.replacedFields.Count; j++)
				{
					var fieldDefinition = Weaver.lists.replacedFields[j];
					if (opField == fieldDefinition)
					{
						i.OpCode = OpCodes.Call;
						i.Operand = Weaver.lists.replacementProperties[j];
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
					Weaver.ProcessInstructionMethod(moduleDef, td, md, i, methodReference, iCount);
				}
			}
			if (i.OpCode == OpCodes.Stfld)
			{
				var fieldDefinition = i.Operand as FieldDefinition;
				if (fieldDefinition != null)
				{
					Weaver.ProcessInstructionField(td, md, i, fieldDefinition);
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
			if (md.ReturnType.FullName != Weaver.voidType.FullName)
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
			if (!Weaver.IsNetworkBehaviour(td))
			{
				Log.Error("[Server] guard on non-NetworkBehaviour script at [" + md.FullName + "]");
			}
			else
			{
				var ilprocessor = md.Body.GetILProcessor();
				var instruction = md.Body.Instructions[0];
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Call, Weaver.NetworkServerGetActive));
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Brtrue, instruction));
				if (logWarning)
				{
					ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Ldstr, "[Server] function '" + md.FullName + "' called on client"));
					ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Call, Weaver.logWarningReference));
				}
				Weaver.InjectGuardParameters(md, ilprocessor, instruction);
				Weaver.InjectGuardReturnValue(md, ilprocessor, instruction);
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Ret));
			}
		}

		private static void InjectClientGuard(ModuleDefinition moduleDef, TypeDefinition td, MethodDefinition md, bool logWarning)
		{
			if (!Weaver.IsNetworkBehaviour(td))
			{
				Log.Error("[Client] guard on non-NetworkBehaviour script at [" + md.FullName + "]");
			}
			else
			{
				var ilprocessor = md.Body.GetILProcessor();
				var instruction = md.Body.Instructions[0];
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Call, Weaver.NetworkClientGetActive));
				ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Brtrue, instruction));
				if (logWarning)
				{
					ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Ldstr, "[Client] function '" + md.FullName + "' called on server"));
					ilprocessor.InsertBefore(instruction, ilprocessor.Create(OpCodes.Call, Weaver.logWarningReference));
				}
				Weaver.InjectGuardParameters(md, ilprocessor, instruction);
				Weaver.InjectGuardReturnValue(md, ilprocessor, instruction);
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
										Weaver.InjectServerGuard(moduleDef, td, md, true);
									}
									else if (customAttribute.Constructor.DeclaringType.ToString() == "UnityEngine.Networking.ServerCallbackAttribute")
									{
										Weaver.InjectServerGuard(moduleDef, td, md, false);
									}
									else if (customAttribute.Constructor.DeclaringType.ToString() == "UnityEngine.Networking.ClientAttribute")
									{
										Weaver.InjectClientGuard(moduleDef, td, md, true);
									}
									else if (customAttribute.Constructor.DeclaringType.ToString() == "UnityEngine.Networking.ClientCallbackAttribute")
									{
										Weaver.InjectClientGuard(moduleDef, td, md, false);
									}
								}
								var num = 0;
								foreach (var i in md.Body.Instructions)
								{
									Weaver.ProcessInstruction(moduleDef, td, md, i, num);
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
				Weaver.ProcessSiteMethod(moduleDef, td, md);
			}
			foreach (var td2 in td.NestedTypes)
			{
				Weaver.ProcessSiteClass(moduleDef, td2);
			}
		}

		private static void ProcessSitesModule(ModuleDefinition moduleDef)
		{
			var now = DateTime.Now;
			foreach (var typeDefinition in moduleDef.Types)
			{
				if (typeDefinition.IsClass)
				{
					Weaver.ProcessSiteClass(moduleDef, typeDefinition);
				}
			}
			if (Weaver.lists.generateContainerClass != null)
			{
				moduleDef.Types.Add(Weaver.lists.generateContainerClass);
				Weaver.scriptDef.MainModule.ImportReference(Weaver.lists.generateContainerClass);
				foreach (var method in Weaver.lists.generatedReadFunctions)
				{
					Weaver.scriptDef.MainModule.ImportReference(method);
				}
				foreach (var method2 in Weaver.lists.generatedWriteFunctions)
				{
					Weaver.scriptDef.MainModule.ImportReference(method2);
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

		private static void ProcessPropertySites()
		{
			Weaver.ProcessSitesModule(Weaver.scriptDef.MainModule);
		}

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
					Weaver.DLog(td, " Already processed", new object[0]);
					return false;
				}
			}
			Weaver.DLog(td, "Found NetworkBehaviour " + td.FullName, new object[0]);
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
				Weaver.fail = true;
				result = null;
			}
			else
			{
				foreach (var methodDefinition in t.Resolve().Methods)
				{
					if (methodDefinition.Name == name)
					{
						return Weaver.scriptDef.MainModule.ImportReference(methodDefinition);
					}
				}
				Log.Error($"ResolveMethod failed - Couldn't find {name} in {t.Name}");
				foreach (var methodDefinition2 in t.Resolve().Methods)
				{
					Log.Error("- has method " + methodDefinition2.Name);
				}
				Weaver.fail = true;
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
							return Weaver.scriptDef.MainModule.ImportReference(methodDefinition);
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
			Weaver.fail = true;
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
							var method = Weaver.scriptDef.MainModule.ImportReference(methodDefinition);
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
			Weaver.fail = true;
			return null;
		}

		public static FieldReference ResolveField(TypeReference t, string name)
		{
			foreach (var fieldDefinition in t.Resolve().Fields)
			{
				if (fieldDefinition.Name == name)
				{
					return Weaver.scriptDef.MainModule.ImportReference(fieldDefinition);
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
					return Weaver.scriptDef.MainModule.ImportReference(propertyDefinition.GetMethod);
				}
			}
			Log.Error($"ResolveProperty failed - Couldn't find {name} in {t.Name}");
			return null;
		}

		private static void SetupUnityTypes()
		{
			Weaver.vector2Type = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Vector2");
			Weaver.vector3Type = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Vector3");
			Weaver.vector4Type = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Vector4");
			Weaver.colorType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Color");
			Weaver.color32Type = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Color32");
			Weaver.quaternionType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Quaternion");
			Weaver.rectType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Rect");
			Weaver.planeType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Plane");
			Weaver.rayType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Ray");
			Weaver.matrixType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Matrix4x4");
			Weaver.gameObjectType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.GameObject");
			Weaver.transformType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Transform");
			Weaver.unityObjectType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Object");

			Weaver.hashType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkHash128");

			Weaver.NetworkClientType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkClient");
			Weaver.NetworkServerType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkServer");
			Weaver.NetworkCRCType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkCRC");

			Weaver.SyncVarType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncVarAttribute");

			Weaver.CommandType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.CommandAttribute");

			Weaver.ClientRpcType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.ClientRpcAttribute");

			Weaver.TargetRpcType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.TargetRpcAttribute");

			Weaver.SyncEventType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncEventAttribute");

			Weaver.SyncListType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncList`1");

			Weaver.NetworkSettingsType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkSettingsAttribute");

			Weaver.SyncListFloatType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListFloat");

			Weaver.SyncListIntType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListInt");

			Weaver.SyncListUIntType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListUInt");

			Weaver.SyncListBoolType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListBool");

			Weaver.SyncListStringType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListString");

		}

		private static void SetupCorLib()
		{
			var name = AssemblyNameReference.Parse("mscorlib");
			var parameters = new ReaderParameters
			{
				AssemblyResolver = Weaver.scriptDef.MainModule.AssemblyResolver
			};
			Weaver.corLib = Weaver.scriptDef.MainModule.AssemblyResolver.Resolve(name, parameters).MainModule;
		}

		private static TypeReference ImportCorLibType(string fullName)
		{
			var type = Weaver.corLib.GetType(fullName) ?? Enumerable.First<ExportedType>(Weaver.corLib.ExportedTypes, (ExportedType t) => t.FullName == fullName).Resolve();
			return Weaver.scriptDef.MainModule.ImportReference(type);
		}

		private static void SetupTargetTypes()
		{
			Weaver.SetupCorLib();
			Weaver.voidType = Weaver.ImportCorLibType("System.Void");
			Weaver.singleType = Weaver.ImportCorLibType("System.Single");
			Weaver.doubleType = Weaver.ImportCorLibType("System.Double");
			Weaver.decimalType = Weaver.ImportCorLibType("System.Decimal");
			Weaver.boolType = Weaver.ImportCorLibType("System.Boolean");
			Weaver.stringType = Weaver.ImportCorLibType("System.String");
			Weaver.int64Type = Weaver.ImportCorLibType("System.Int64");
			Weaver.uint64Type = Weaver.ImportCorLibType("System.UInt64");
			Weaver.int32Type = Weaver.ImportCorLibType("System.Int32");
			Weaver.uint32Type = Weaver.ImportCorLibType("System.UInt32");
			Weaver.int16Type = Weaver.ImportCorLibType("System.Int16");
			Weaver.uint16Type = Weaver.ImportCorLibType("System.UInt16");
			Weaver.byteType = Weaver.ImportCorLibType("System.Byte");
			Weaver.sbyteType = Weaver.ImportCorLibType("System.SByte");
			Weaver.charType = Weaver.ImportCorLibType("System.Char");
			Weaver.objectType = Weaver.ImportCorLibType("System.Object");
			Weaver.valueTypeType = Weaver.ImportCorLibType("System.ValueType");
			Weaver.typeType = Weaver.ImportCorLibType("System.Type");
			Weaver.IEnumeratorType = Weaver.ImportCorLibType("System.Collections.IEnumerator");
			Weaver.MemoryStreamType = Weaver.ImportCorLibType("System.IO.MemoryStream");

			Weaver.NetworkReaderType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.Transport.QNetworkReader");

			Weaver.NetworkReaderDef = Weaver.NetworkReaderType.Resolve();
			Weaver.NetworkReaderCtor = Weaver.ResolveMethod(Weaver.NetworkReaderDef, ".ctor");

			Weaver.NetworkWriterType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.Transport.QNetworkWriter");

			Weaver.NetworkWriterDef = Weaver.NetworkWriterType.Resolve();
			Weaver.NetworkWriterCtor = Weaver.ResolveMethod(Weaver.NetworkWriterDef, ".ctor");
			Weaver.MemoryStreamCtor = Weaver.ResolveMethod(Weaver.MemoryStreamType, ".ctor");

			Weaver.NetworkInstanceIdType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkInstanceId");
			Weaver.NetworkSceneIdType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkSceneId");
			Weaver.NetworkInstanceIdType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkInstanceId");
			Weaver.NetworkSceneIdType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.NetworkSceneId");

			Weaver.NetworkServerGetActive = Weaver.ResolveMethod(Weaver.NetworkServerType, "get_active");
			Weaver.NetworkServerGetLocalClientActive = Weaver.ResolveMethod(Weaver.NetworkServerType, "get_localClientActive");
			Weaver.NetworkClientGetActive = Weaver.ResolveMethod(Weaver.NetworkClientType, "get_active");
			Weaver.NetworkReaderReadInt32 = Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadInt32");
			Weaver.NetworkWriterWriteInt32 = Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.int32Type);
			Weaver.NetworkWriterWriteInt16 = Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.int16Type);
			Weaver.NetworkReaderReadPacked32 = Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadPackedUInt32");
			Weaver.NetworkReaderReadPacked64 = Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadPackedUInt64");
			Weaver.NetworkReaderReadByte = Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadByte");
			Weaver.NetworkWriterWritePacked32 = Weaver.ResolveMethod(Weaver.NetworkWriterType, "WritePackedUInt32");
			Weaver.NetworkWriterWritePacked64 = Weaver.ResolveMethod(Weaver.NetworkWriterType, "WritePackedUInt64");
			Weaver.NetworkWriterWriteNetworkInstanceId = Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.NetworkInstanceIdType);
			Weaver.NetworkWriterWriteNetworkSceneId = Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.NetworkSceneIdType);
			Weaver.NetworkReaderReadNetworkInstanceId = Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadNetworkId");
			Weaver.NetworkReaderReadNetworkSceneId = Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadSceneId");
			Weaver.NetworkInstanceIsEmpty = Weaver.ResolveMethod(Weaver.NetworkInstanceIdType, "IsEmpty");
			Weaver.NetworkReadUInt16 = Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadUInt16");
			Weaver.NetworkWriteUInt16 = Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.uint16Type);

			Weaver.CmdDelegateReference = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkBehaviour/CmdDelegate");

			Weaver.CmdDelegateConstructor = Weaver.ResolveMethod(Weaver.CmdDelegateReference, ".ctor");
			Weaver.scriptDef.MainModule.ImportReference(Weaver.gameObjectType);
			Weaver.scriptDef.MainModule.ImportReference(Weaver.transformType);

			TypeReference type = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.Components.QNetworkIdentity");

			Weaver.NetworkIdentityType = Weaver.scriptDef.MainModule.ImportReference(type);
			Weaver.NetworkInstanceIdType = Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkInstanceIdType);
			Weaver.SyncListFloatReadType = Weaver.ResolveMethod(Weaver.SyncListFloatType, "ReadReference");
			Weaver.SyncListIntReadType = Weaver.ResolveMethod(Weaver.SyncListIntType, "ReadReference");
			Weaver.SyncListUIntReadType = Weaver.ResolveMethod(Weaver.SyncListUIntType, "ReadReference");
			Weaver.SyncListBoolReadType = Weaver.ResolveMethod(Weaver.SyncListBoolType, "ReadReference");
			Weaver.SyncListStringReadType = Weaver.ResolveMethod(Weaver.SyncListStringType, "ReadReference");
			Weaver.SyncListFloatWriteType = Weaver.ResolveMethod(Weaver.SyncListFloatType, "WriteInstance");
			Weaver.SyncListIntWriteType = Weaver.ResolveMethod(Weaver.SyncListIntType, "WriteInstance");
			Weaver.SyncListUIntWriteType = Weaver.ResolveMethod(Weaver.SyncListUIntType, "WriteInstance");
			Weaver.SyncListBoolWriteType = Weaver.ResolveMethod(Weaver.SyncListBoolType, "WriteInstance");
			Weaver.SyncListStringWriteType = Weaver.ResolveMethod(Weaver.SyncListStringType, "WriteInstance");

			Weaver.NetworkBehaviourType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkBehaviour");

			Weaver.NetworkBehaviourType2 = Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkBehaviourType);

			Weaver.NetworkConnectionType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkConnection");

			Weaver.MonoBehaviourType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.MonoBehaviour");
			Weaver.ScriptableObjectType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.ScriptableObject");

			Weaver.NetworkConnectionType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QNetworkConnection");

			Weaver.NetworkConnectionType = Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkConnectionType);

			Weaver.ULocalConnectionToServerType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QULocalConnectionToServer");

			Weaver.ULocalConnectionToServerType = Weaver.scriptDef.MainModule.ImportReference(Weaver.ULocalConnectionToServerType);

			Weaver.ULocalConnectionToClientType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QULocalConnectionToClient");

			Weaver.ULocalConnectionToClientType = Weaver.scriptDef.MainModule.ImportReference(Weaver.ULocalConnectionToClientType);

			Weaver.MessageBaseType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.Messages.QMessageBase");
			Weaver.SyncListStructType = Weaver.UNetAssemblyDefinition.MainModule.GetType("UnityEngine.Networking.SyncListStruct`1");

			Weaver.NetworkBehaviourDirtyBitsReference = Weaver.ResolveProperty(Weaver.NetworkBehaviourType, "SyncVarDirtyBits");
			Weaver.ComponentType = Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Component");

			Weaver.ClientSceneType = Weaver.QNetAssemblyDefinition.MainModule.GetType("QuantumUNET.QClientScene");

			Weaver.FindLocalObjectReference = Weaver.ResolveMethod(Weaver.ClientSceneType, "FindLocalObject");
			Weaver.RegisterBehaviourReference = Weaver.ResolveMethod(Weaver.NetworkCRCType, "RegisterBehaviour");
			Weaver.ReadyConnectionReference = Weaver.ResolveMethod(Weaver.ClientSceneType, "get_readyConnection");
			Weaver.getComponentReference = Weaver.ResolveMethodGeneric(Weaver.ComponentType, "GetComponent", Weaver.NetworkIdentityType);
			Weaver.getUNetIdReference = Weaver.ResolveMethod(type, "get_NetId");
			Weaver.gameObjectInequality = Weaver.ResolveMethod(Weaver.unityObjectType, "op_Inequality");
			Weaver.UBehaviourIsServer = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "get_IsServer");
			Weaver.getPlayerIdReference = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "get_PlayerControllerId");
			Weaver.setSyncVarReference = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "SetSyncVar");
			Weaver.setSyncVarHookGuard = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "set_SyncVarHookGuard");
			Weaver.getSyncVarHookGuard = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "get_SyncVarHookGuard");
			Weaver.setSyncVarGameObjectReference = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "SetSyncVarGameObject");
			Weaver.registerCommandDelegateReference = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "RegisterCommandDelegate");
			Weaver.registerRpcDelegateReference = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "RegisterRpcDelegate");
			Weaver.registerEventDelegateReference = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "RegisterEventDelegate");
			Weaver.registerSyncListDelegateReference = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "RegisterSyncListDelegate");
			Weaver.getTypeReference = Weaver.ResolveMethod(Weaver.objectType, "GetType");
			Weaver.getTypeFromHandleReference = Weaver.ResolveMethod(Weaver.typeType, "GetTypeFromHandle");
			Weaver.logErrorReference = Weaver.ResolveMethod(Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Debug"), "LogError");
			Weaver.logWarningReference = Weaver.ResolveMethod(Weaver.UnityAssemblyDefinition.MainModule.GetType("UnityEngine.Debug"), "LogWarning");
			Weaver.sendCommandInternal = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "SendCommandInternal");
			Weaver.sendRpcInternal = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "SendRPCInternal");
			Weaver.sendTargetRpcInternal = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "SendTargetRPCInternal");
			Weaver.sendEventInternal = Weaver.ResolveMethod(Weaver.NetworkBehaviourType, "SendEventInternal");
			Weaver.SyncListType = Weaver.scriptDef.MainModule.ImportReference(Weaver.SyncListType);
			Weaver.SyncListInitBehaviourReference = Weaver.ResolveMethod(Weaver.SyncListType, "InitializeBehaviour");
			Weaver.SyncListInitHandleMsg = Weaver.ResolveMethod(Weaver.SyncListType, "HandleMsg");
			Weaver.SyncListClear = Weaver.ResolveMethod(Weaver.SyncListType, "Clear");
		}

		private static void SetupReadFunctions()
		{
			var weaverLists = Weaver.lists;
			var dictionary = new Dictionary<string, MethodReference>();
			dictionary.Add(Weaver.singleType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadSingle"));
			dictionary.Add(Weaver.doubleType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadDouble"));
			dictionary.Add(Weaver.boolType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadBoolean"));
			dictionary.Add(Weaver.stringType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadString"));
			dictionary.Add(Weaver.int64Type.FullName, Weaver.NetworkReaderReadPacked64);
			dictionary.Add(Weaver.uint64Type.FullName, Weaver.NetworkReaderReadPacked64);
			dictionary.Add(Weaver.int32Type.FullName, Weaver.NetworkReaderReadPacked32);
			dictionary.Add(Weaver.uint32Type.FullName, Weaver.NetworkReaderReadPacked32);
			dictionary.Add(Weaver.int16Type.FullName, Weaver.NetworkReaderReadPacked32);
			dictionary.Add(Weaver.uint16Type.FullName, Weaver.NetworkReaderReadPacked32);
			dictionary.Add(Weaver.byteType.FullName, Weaver.NetworkReaderReadPacked32);
			dictionary.Add(Weaver.sbyteType.FullName, Weaver.NetworkReaderReadPacked32);
			dictionary.Add(Weaver.charType.FullName, Weaver.NetworkReaderReadPacked32);
			dictionary.Add(Weaver.decimalType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadDecimal"));
			dictionary.Add(Weaver.vector2Type.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadVector2"));
			dictionary.Add(Weaver.vector3Type.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadVector3"));
			dictionary.Add(Weaver.vector4Type.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadVector4"));
			dictionary.Add(Weaver.colorType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadColor"));
			dictionary.Add(Weaver.color32Type.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadColor32"));
			dictionary.Add(Weaver.quaternionType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadQuaternion"));
			dictionary.Add(Weaver.rectType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadRect"));
			dictionary.Add(Weaver.planeType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadPlane"));
			dictionary.Add(Weaver.rayType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadRay"));
			dictionary.Add(Weaver.matrixType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadMatrix4x4"));
			dictionary.Add(Weaver.hashType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadNetworkHash128"));
			dictionary.Add(Weaver.gameObjectType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadGameObject"));
			dictionary.Add(Weaver.NetworkIdentityType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadNetworkIdentity"));
			dictionary.Add(Weaver.NetworkInstanceIdType.FullName, Weaver.NetworkReaderReadNetworkInstanceId);
			dictionary.Add(Weaver.NetworkSceneIdType.FullName, Weaver.NetworkReaderReadNetworkSceneId);
			dictionary.Add(Weaver.transformType.FullName, Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadTransform"));
			dictionary.Add("System.Byte[]", Weaver.ResolveMethod(Weaver.NetworkReaderType, "ReadBytesAndSize"));
			weaverLists.readFuncs = dictionary;
			var weaverLists2 = Weaver.lists;
			dictionary = new Dictionary<string, MethodReference>();
			dictionary.Add(Weaver.SyncListFloatType.FullName, Weaver.SyncListFloatReadType);
			dictionary.Add(Weaver.SyncListIntType.FullName, Weaver.SyncListIntReadType);
			dictionary.Add(Weaver.SyncListUIntType.FullName, Weaver.SyncListUIntReadType);
			dictionary.Add(Weaver.SyncListBoolType.FullName, Weaver.SyncListBoolReadType);
			dictionary.Add(Weaver.SyncListStringType.FullName, Weaver.SyncListStringReadType);
			weaverLists2.readByReferenceFuncs = dictionary;
		}

		private static void SetupWriteFunctions()
		{
			var weaverLists = Weaver.lists;
			var dictionary = new Dictionary<string, MethodReference>();
			dictionary.Add(Weaver.singleType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.singleType));
			dictionary.Add(Weaver.doubleType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.doubleType));
			dictionary.Add(Weaver.boolType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.boolType));
			dictionary.Add(Weaver.stringType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.stringType));
			dictionary.Add(Weaver.int64Type.FullName, Weaver.NetworkWriterWritePacked64);
			dictionary.Add(Weaver.uint64Type.FullName, Weaver.NetworkWriterWritePacked64);
			dictionary.Add(Weaver.int32Type.FullName, Weaver.NetworkWriterWritePacked32);
			dictionary.Add(Weaver.uint32Type.FullName, Weaver.NetworkWriterWritePacked32);
			dictionary.Add(Weaver.int16Type.FullName, Weaver.NetworkWriterWritePacked32);
			dictionary.Add(Weaver.uint16Type.FullName, Weaver.NetworkWriterWritePacked32);
			dictionary.Add(Weaver.byteType.FullName, Weaver.NetworkWriterWritePacked32);
			dictionary.Add(Weaver.sbyteType.FullName, Weaver.NetworkWriterWritePacked32);
			dictionary.Add(Weaver.charType.FullName, Weaver.NetworkWriterWritePacked32);
			dictionary.Add(Weaver.decimalType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.decimalType));
			dictionary.Add(Weaver.vector2Type.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.vector2Type));
			dictionary.Add(Weaver.vector3Type.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.vector3Type));
			dictionary.Add(Weaver.vector4Type.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.vector4Type));
			dictionary.Add(Weaver.colorType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.colorType));
			dictionary.Add(Weaver.color32Type.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.color32Type));
			dictionary.Add(Weaver.quaternionType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.quaternionType));
			dictionary.Add(Weaver.rectType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.rectType));
			dictionary.Add(Weaver.planeType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.planeType));
			dictionary.Add(Weaver.rayType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.rayType));
			dictionary.Add(Weaver.matrixType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.matrixType));
			dictionary.Add(Weaver.hashType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.hashType));
			dictionary.Add(Weaver.gameObjectType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.gameObjectType));
			dictionary.Add(Weaver.NetworkIdentityType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.NetworkIdentityType));
			dictionary.Add(Weaver.NetworkInstanceIdType.FullName, Weaver.NetworkWriterWriteNetworkInstanceId);
			dictionary.Add(Weaver.NetworkSceneIdType.FullName, Weaver.NetworkWriterWriteNetworkSceneId);
			dictionary.Add(Weaver.transformType.FullName, Weaver.ResolveMethodWithArg(Weaver.NetworkWriterType, "Write", Weaver.transformType));
			dictionary.Add("System.Byte[]", Weaver.ResolveMethod(Weaver.NetworkWriterType, "WriteBytesFull"));
			dictionary.Add(Weaver.SyncListFloatType.FullName, Weaver.SyncListFloatWriteType);
			dictionary.Add(Weaver.SyncListIntType.FullName, Weaver.SyncListIntWriteType);
			dictionary.Add(Weaver.SyncListUIntType.FullName, Weaver.SyncListUIntWriteType);
			dictionary.Add(Weaver.SyncListBoolType.FullName, Weaver.SyncListBoolWriteType);
			dictionary.Add(Weaver.SyncListStringType.FullName, Weaver.SyncListStringWriteType);
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
					if (baseType.FullName == Weaver.NetworkBehaviourType.FullName)
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
			var name = Weaver.scriptDef.MainModule.Name;
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
				Weaver.fail = true;
				Weaver.fail = true;
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
			if (Weaver.IsDerivedFrom(td, Weaver.MonoBehaviourType))
			{
				Weaver.ProcessMonoBehaviourType(td);
			}
		}

		private static bool CheckNetworkBehaviour(TypeDefinition td)
		{
			bool result;
			if (!td.IsClass)
			{
				result = false;
			}
			else if (!Weaver.IsNetworkBehaviour(td))
			{
				Weaver.CheckMonoBehaviour(td);
				result = false;
			}
			else
			{
				var list = new List<TypeDefinition>();
				var typeDefinition = td;
				while (typeDefinition != null)
				{
					if (typeDefinition.FullName == Weaver.NetworkBehaviourType.FullName)
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
					flag |= Weaver.ProcessNetworkBehaviourType(td2);
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
					if (baseType.FullName == Weaver.MessageBaseType.FullName)
					{
						flag |= Weaver.ProcessMessageType(td);
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
					flag |= Weaver.CheckMessageBase(td2);
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
						flag |= Weaver.ProcessSyncListStructType(td);
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
					flag |= Weaver.CheckSyncListStruct(td2);
				}
				result = flag;
			}
			return result;
		}

		private static bool Weave(string assName, IEnumerable<string> dependencies, IAssemblyResolver assemblyResolver, string unityEngineDLLPath, string unityUNetDLLPath, string outputDir)
		{
			var readerParameters = Helpers.ReaderParameters(assName, dependencies, assemblyResolver, unityEngineDLLPath, unityUNetDLLPath);
			Weaver.scriptDef = AssemblyDefinition.ReadAssembly(assName, readerParameters);
			Weaver.SetupTargetTypes();
			Weaver.SetupReadFunctions();
			Weaver.SetupWriteFunctions();
			var mainModule = Weaver.scriptDef.MainModule;
			Console.WriteLine("Script Module: {0}", mainModule.Name);
			var flag = false;
			for (var i = 0; i < 2; i++)
			{
				var stopwatch = Stopwatch.StartNew();
				foreach (var typeDefinition in mainModule.Types)
				{
					if (typeDefinition.IsClass && Weaver.CanBeResolved(typeDefinition.BaseType))
					{
						try
						{
							if (i == 0)
							{
								flag |= Weaver.CheckSyncListStruct(typeDefinition);
							}
							else
							{
								flag |= Weaver.CheckNetworkBehaviour(typeDefinition);
								flag |= Weaver.CheckMessageBase(typeDefinition);
							}
						}
						catch (Exception ex)
						{
							if (Weaver.scriptDef.MainModule.SymbolReader != null)
							{
								Weaver.scriptDef.MainModule.SymbolReader.Dispose();
							}
							Weaver.fail = true;
							throw ex;
						}
					}
					if (Weaver.fail)
					{
						if (Weaver.scriptDef.MainModule.SymbolReader != null)
						{
							Weaver.scriptDef.MainModule.SymbolReader.Dispose();
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
				foreach (var methodDefinition in Weaver.lists.replacedMethods)
				{
					Weaver.lists.replacementMethodNames.Add(methodDefinition.FullName);
				}
				try
				{
					Weaver.ProcessPropertySites();
				}
				catch (Exception ex2)
				{
					Log.Error("ProcessPropertySites exception: " + ex2);
					if (Weaver.scriptDef.MainModule.SymbolReader != null)
					{
						Weaver.scriptDef.MainModule.SymbolReader.Dispose();
					}
					return false;
				}
				if (Weaver.fail)
				{
					if (Weaver.scriptDef.MainModule.SymbolReader != null)
					{
						Weaver.scriptDef.MainModule.SymbolReader.Dispose();
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
				Weaver.scriptDef.Write(fileName, writerParameters);
			}
			if (Weaver.scriptDef.MainModule.SymbolReader != null)
			{
				Weaver.scriptDef.MainModule.SymbolReader.Dispose();
			}
			return true;
		}

		public static bool WeaveAssemblies(string assembly, IEnumerable<string> dependencies, IAssemblyResolver assemblyResolver, string outputDir, string unityEngineDLLPath, string unityQNetDLLPath, string unityUNetDLLPath)
		{
			Weaver.fail = false;
			Weaver.lists = new WeaverLists();
			Weaver.UnityAssemblyDefinition = AssemblyDefinition.ReadAssembly(unityEngineDLLPath);
			Weaver.QNetAssemblyDefinition = AssemblyDefinition.ReadAssembly(unityQNetDLLPath);
			Weaver.UNetAssemblyDefinition = AssemblyDefinition.ReadAssembly(unityUNetDLLPath);
			Weaver.SetupUnityTypes();
			try
			{
				if (!Weaver.Weave(assembly, dependencies, assemblyResolver, unityEngineDLLPath, unityQNetDLLPath, outputDir))
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception :" + ex);
				return false;
			}
			Weaver.corLib = null;
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
