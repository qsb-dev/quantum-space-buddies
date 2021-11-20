using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QNetWeaver
{
	internal class NetworkBehaviourProcessor
	{
		public NetworkBehaviourProcessor(TypeDefinition td)
		{
			Weaver.DLog(td, "NetworkBehaviourProcessor", new object[0]);
			m_td = td;
		}

		public void Process()
		{
			if (m_td.HasGenericParameters)
			{
				Weaver.fail = true;
				Log.Error("NetworkBehaviour " + m_td.Name + " cannot have generic parameters");
			}
			else
			{
				Weaver.DLog(m_td, "Process Start", new object[0]);
				ProcessVersion();
				ProcessSyncVars();
				Weaver.ResetRecursionCount();
				ProcessMethods();
				ProcessEvents();
				if (!Weaver.fail)
				{
					GenerateNetworkSettings();
					GenerateConstants();
					Weaver.ResetRecursionCount();
					GenerateSerialization();
					if (!Weaver.fail)
					{
						GenerateDeSerialization();
						GeneratePreStartClient();
						Weaver.DLog(m_td, "Process Done", new object[0]);
					}
				}
			}
		}

		private static void WriteClientActiveCheck(ILProcessor worker, string mdName, Instruction label, string errString)
		{
			worker.Append(worker.Create(OpCodes.Call, Weaver.NetworkClientGetActive));
			worker.Append(worker.Create(OpCodes.Brtrue, label));
			worker.Append(worker.Create(OpCodes.Ldstr, errString + " " + mdName + " called on server."));
			worker.Append(worker.Create(OpCodes.Call, Weaver.logErrorReference));
			worker.Append(worker.Create(OpCodes.Ret));
			worker.Append(label);
		}

		private static void WriteServerActiveCheck(ILProcessor worker, string mdName, Instruction label, string errString)
		{
			worker.Append(worker.Create(OpCodes.Call, Weaver.NetworkServerGetActive));
			worker.Append(worker.Create(OpCodes.Brtrue, label));
			worker.Append(worker.Create(OpCodes.Ldstr, errString + " " + mdName + " called on client."));
			worker.Append(worker.Create(OpCodes.Call, Weaver.logErrorReference));
			worker.Append(worker.Create(OpCodes.Ret));
			worker.Append(label);
		}

		private static void WriteSetupLocals(ILProcessor worker)
		{
			worker.Body.InitLocals = true;
			worker.Body.Variables.Add(new VariableDefinition(Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkWriterType)));
		}

		private static void WriteCreateWriter(ILProcessor worker)
		{
			worker.Append(worker.Create(OpCodes.Newobj, Weaver.NetworkWriterCtor));
			worker.Append(worker.Create(OpCodes.Stloc_0));
			worker.Append(worker.Create(OpCodes.Ldloc_0));
		}

		private static void WriteMessageSize(ILProcessor worker)
		{
			worker.Append(worker.Create(OpCodes.Ldc_I4_0));
			worker.Append(worker.Create(OpCodes.Callvirt, Weaver.NetworkWriterWriteInt16));
		}

		private static void WriteMessageId(ILProcessor worker, int msgId)
		{
			worker.Append(worker.Create(OpCodes.Ldloc_0));
			worker.Append(worker.Create(OpCodes.Ldc_I4, msgId));
			worker.Append(worker.Create(OpCodes.Conv_U2));
			worker.Append(worker.Create(OpCodes.Callvirt, Weaver.NetworkWriterWriteInt16));
		}

		private static bool WriteArguments(ILProcessor worker, MethodDefinition md, string errString, bool skipFirst)
		{
			short num = 1;
			foreach (var parameterDefinition in md.Parameters)
			{
				if (num == 1 && skipFirst)
				{
					num += 1;
				}
				else
				{
					var writeFunc = Weaver.GetWriteFunc(parameterDefinition.ParameterType);
					if (writeFunc == null)
					{
						Log.Error(string.Concat(new object[]
						{
							"WriteArguments for ",
							md.Name,
							" type ",
							parameterDefinition.ParameterType,
							" not supported"
						}));
						Weaver.fail = true;
						return false;
					}

					worker.Append(worker.Create(OpCodes.Ldloc_0));
					worker.Append(worker.Create(OpCodes.Ldarg, num));
					worker.Append(worker.Create(OpCodes.Call, writeFunc));
					num += 1;
				}
			}

			return true;
		}

		private void ProcessVersion()
		{
			foreach (var methodDefinition in m_td.Methods)
			{
				if (methodDefinition.Name == "UNetVersion")
				{
					return;
				}
			}

			var methodDefinition2 = new MethodDefinition("UNetVersion", MethodAttributes.Private, Weaver.voidType);
			var ilprocessor = methodDefinition2.Body.GetILProcessor();
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			m_td.Methods.Add(methodDefinition2);
		}

		private void GenerateConstants()
		{
			if (m_Cmds.Count != 0 || m_Rpcs.Count != 0 || m_TargetRpcs.Count != 0 || m_Events.Count != 0 || m_SyncLists.Count != 0)
			{
				Weaver.DLog(m_td, "  GenerateConstants ", new object[0]);
				MethodDefinition cctorMethodDef = null;

				var flag = false;
				foreach (var methodDef in m_td.Methods)
				{
					if (methodDef.Name == ".cctor")
					{
						cctorMethodDef = methodDef;
						flag = true;
					}
				}

				if (cctorMethodDef != null)
				{
					if (cctorMethodDef.Body.Instructions.Count != 0)
					{
						var returnInstruction = cctorMethodDef.Body.Instructions[cctorMethodDef.Body.Instructions.Count - 1];
						if (!(returnInstruction.OpCode == OpCodes.Ret))
						{
							Log.Error("No .cctor for " + m_td.Name);
							Weaver.fail = true;
							return;
						}

						cctorMethodDef.Body.Instructions.RemoveAt(cctorMethodDef.Body.Instructions.Count - 1);
					}
				}
				else
				{
					Weaver.DLog(m_td, "  No. cctor found, making... ", new object[0]);
					cctorMethodDef = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, Weaver.voidType);
				}

				MethodDefinition ctorMethodDef = null;
				foreach (var methodDef in m_td.Methods)
				{
					if (methodDef.Name == ".ctor")
					{
						ctorMethodDef = methodDef;
						var returnInstruction = ctorMethodDef.Body.Instructions[ctorMethodDef.Body.Instructions.Count - 1];
						if (returnInstruction.OpCode == OpCodes.Ret)
						{
							Weaver.DLog(m_td, "  Found .ctor ", new object[0]);
							ctorMethodDef.Body.Instructions.RemoveAt(ctorMethodDef.Body.Instructions.Count - 1);
							break;
						}

						Weaver.fail = true;
						Log.Error("No .ctor for " + m_td.Name);
						return;
					}
				}

				if (ctorMethodDef == null)
				{
					Weaver.fail = true;
					Log.Error("No .ctor for " + m_td.Name);
				}
				else
				{
					var ilprocessor = ctorMethodDef.Body.GetILProcessor();
					var ilprocessor2 = cctorMethodDef.Body.GetILProcessor();

					var commandIndex = 0;
					foreach (var commandDef in m_Cmds)
					{
						Weaver.DLog(m_td, $"  Found command {commandDef.Name}", new object[0]);
						var field = Weaver.ResolveField(m_td, "kCmd" + commandDef.Name);
						Weaver.DLog(m_td, $"    got field", new object[0]);
						var hashCode = GetHashCode(m_td.Name + ":Cmd:" + commandDef.Name);
						Weaver.DLog(m_td, $"    got hashcode", new object[0]);
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Ldc_I4, hashCode));
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Stsfld, field));

						GenerateCommandDelegate(ilprocessor2, Weaver.registerCommandDelegateReference, m_CmdInvocationFuncs[commandIndex], field);
						commandIndex++;
					}

					var rpcIndex = 0;
					foreach (var rpcDef in m_Rpcs)
					{
						var field2 = Weaver.ResolveField(m_td, "kRpc" + rpcDef.Name);
						var hashCode2 = GetHashCode(m_td.Name + ":Rpc:" + rpcDef.Name);
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Ldc_I4, hashCode2));
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Stsfld, field2));
						GenerateCommandDelegate(ilprocessor2, Weaver.registerRpcDelegateReference, m_RpcInvocationFuncs[rpcIndex], field2);
						rpcIndex++;
					}

					var targetRpcIndex = 0;
					foreach (var targetRpcDef in m_TargetRpcs)
					{
						var field3 = Weaver.ResolveField(m_td, "kTargetRpc" + targetRpcDef.Name);
						var hashCode3 = GetHashCode(m_td.Name + ":TargetRpc:" + targetRpcDef.Name);
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Ldc_I4, hashCode3));
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Stsfld, field3));
						GenerateCommandDelegate(ilprocessor2, Weaver.registerRpcDelegateReference, m_TargetRpcInvocationFuncs[targetRpcIndex], field3);
						targetRpcIndex++;
					}

					var eventIndex = 0;
					foreach (var eventDef in m_Events)
					{
						var field4 = Weaver.ResolveField(m_td, "kEvent" + eventDef.Name);
						var hashCode4 = GetHashCode(m_td.Name + ":Event:" + eventDef.Name);
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Ldc_I4, hashCode4));
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Stsfld, field4));
						GenerateCommandDelegate(ilprocessor2, Weaver.registerEventDelegateReference, m_EventInvocationFuncs[eventIndex], field4);
						eventIndex++;
					}

					var syncListIndex = 0;
					foreach (var syncListDef in m_SyncLists)
					{
						var field5 = Weaver.ResolveField(m_td, "kList" + syncListDef.Name);
						var hashCode5 = GetHashCode(m_td.Name + ":List:" + syncListDef.Name);
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Ldc_I4, hashCode5));
						ilprocessor2.Append(ilprocessor2.Create(OpCodes.Stsfld, field5));
						GenerateSyncListInstanceInitializer(ilprocessor, syncListDef);
						GenerateCommandDelegate(ilprocessor2, Weaver.registerSyncListDelegateReference, m_SyncListInvocationFuncs[syncListIndex], field5);
						syncListIndex++;
					}

					ilprocessor2.Append(ilprocessor2.Create(OpCodes.Ldstr, m_td.Name));
					ilprocessor2.Append(ilprocessor2.Create(OpCodes.Ldc_I4, m_QosChannel));
					ilprocessor2.Append(ilprocessor2.Create(OpCodes.Call, Weaver.RegisterBehaviourReference));
					ilprocessor2.Append(ilprocessor2.Create(OpCodes.Ret));
					if (!flag)
					{
						m_td.Methods.Add(cctorMethodDef);
					}

					ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
					m_td.Attributes &= ~TypeAttributes.BeforeFieldInit;
					if (m_SyncLists.Count != 0)
					{
						MethodDefinition methodDefinition8 = null;
						var flag2 = false;
						foreach (var methodDefinition9 in m_td.Methods)
						{
							if (methodDefinition9.Name == "Awake")
							{
								methodDefinition8 = methodDefinition9;
								flag2 = true;
							}
						}

						if (methodDefinition8 != null)
						{
							if (methodDefinition8.Body.Instructions.Count != 0)
							{
								var instruction3 = methodDefinition8.Body.Instructions[methodDefinition8.Body.Instructions.Count - 1];
								if (!(instruction3.OpCode == OpCodes.Ret))
								{
									Log.Error("No awake for " + m_td.Name);
									Weaver.fail = true;
									return;
								}

								methodDefinition8.Body.Instructions.RemoveAt(methodDefinition8.Body.Instructions.Count - 1);
							}
						}
						else
						{
							methodDefinition8 = new MethodDefinition("Awake", MethodAttributes.Private, Weaver.voidType);
						}

						var ilprocessor3 = methodDefinition8.Body.GetILProcessor();
						if (!flag2)
						{
							CheckForCustomBaseClassAwakeMethod(ilprocessor3);
						}

						var num6 = 0;
						foreach (var fd in m_SyncLists)
						{
							GenerateSyncListInitializer(ilprocessor3, fd, num6);
							num6++;
						}

						ilprocessor3.Append(ilprocessor3.Create(OpCodes.Ret));
						if (!flag2)
						{
							m_td.Methods.Add(methodDefinition8);
						}
					}
				}
			}
		}

		private void CheckForCustomBaseClassAwakeMethod(ILProcessor awakeWorker)
		{
			var baseType = m_td.BaseType;
			while (baseType.FullName != Weaver.NetworkBehaviourType.FullName)
			{
				var methodDefinition = Enumerable.FirstOrDefault<MethodDefinition>(baseType.Resolve().Methods, (MethodDefinition x) => x.Name == "Awake" && !x.HasParameters);
				if (methodDefinition != null)
				{
					awakeWorker.Append(awakeWorker.Create(OpCodes.Ldarg_0));
					awakeWorker.Append(awakeWorker.Create(OpCodes.Call, methodDefinition));
					break;
				}

				baseType = baseType.Resolve().BaseType;
			}
		}

		private void GenerateSyncListInstanceInitializer(ILProcessor ctorWorker, FieldDefinition fd)
		{
			foreach (var instruction in ctorWorker.Body.Instructions)
			{
				if (instruction.OpCode.Code == Code.Stfld)
				{
					var fieldDefinition = (FieldDefinition)instruction.Operand;
					if (fieldDefinition.DeclaringType == fd.DeclaringType && fieldDefinition.Name == fd.Name)
					{
						return;
					}
				}
			}

			var method = Weaver.scriptDef.MainModule.ImportReference(Enumerable.First<MethodDefinition>(fd.FieldType.Resolve().Methods, (MethodDefinition x) => x.Name == ".ctor" && !x.HasParameters));
			ctorWorker.Append(ctorWorker.Create(OpCodes.Ldarg_0));
			ctorWorker.Append(ctorWorker.Create(OpCodes.Newobj, method));
			ctorWorker.Append(ctorWorker.Create(OpCodes.Stfld, fd));
		}

		private void GenerateCommandDelegate(ILProcessor awakeWorker, MethodReference registerMethod, MethodDefinition func, FieldReference field)
		{
			Weaver.DLog(m_td, "  GenerateCommandDelegate ", new object[0]);
			awakeWorker.Append(awakeWorker.Create(OpCodes.Ldtoken, m_td));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Call, Weaver.getTypeFromHandleReference));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Ldsfld, field));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Ldnull));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Ldftn, func));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Newobj, Weaver.CmdDelegateConstructor));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Call, registerMethod));
		}

		private void GenerateSyncListInitializer(ILProcessor awakeWorker, FieldReference fd, int index)
		{
			awakeWorker.Append(awakeWorker.Create(OpCodes.Ldarg_0));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Ldfld, fd));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Ldarg_0));
			awakeWorker.Append(awakeWorker.Create(OpCodes.Ldsfld, m_SyncListStaticFields[index]));
			var genericInstanceType = (GenericInstanceType)fd.FieldType.Resolve().BaseType;
			genericInstanceType = (GenericInstanceType)Weaver.scriptDef.MainModule.ImportReference(genericInstanceType);
			var typeReference = genericInstanceType.GenericArguments[0];
			var method = Helpers.MakeHostInstanceGeneric(Weaver.SyncListInitBehaviourReference, new TypeReference[]
			{
				typeReference
			});
			awakeWorker.Append(awakeWorker.Create(OpCodes.Callvirt, method));
			Weaver.scriptDef.MainModule.ImportReference(method);
		}

		private void GenerateSerialization()
		{
			Weaver.DLog(m_td, "  NetworkBehaviour GenerateSerialization", new object[0]);
			foreach (var methodDefinition in m_td.Methods)
			{
				if (methodDefinition.Name == "OnSerialize")
				{
					Weaver.DLog(m_td, "    Abort - is OnSerialize", new object[0]);
					return;
				}
			}

			var methodDefinition2 = new MethodDefinition("OnSerialize", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, Weaver.boolType);
			methodDefinition2.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkWriterType)));
			methodDefinition2.Parameters.Add(new ParameterDefinition("forceAll", ParameterAttributes.None, Weaver.boolType));
			var ilprocessor = methodDefinition2.Body.GetILProcessor();
			methodDefinition2.Body.InitLocals = true;
			var item = new VariableDefinition(Weaver.boolType);
			methodDefinition2.Body.Variables.Add(item);
			var flag = false;

			if (m_td.BaseType.FullName != Weaver.NetworkBehaviourType.FullName)
			{
				var methodReference = Weaver.ResolveMethod(m_td.BaseType, "OnSerialize");
				if (methodReference != null)
				{
					var item2 = new VariableDefinition(Weaver.boolType);
					methodDefinition2.Body.Variables.Add(item2);
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_2));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, methodReference));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_1));
					flag = true;
				}
			}

			if (m_SyncVars.Count == 0)
			{
				Weaver.DLog(m_td, "    No syncvars", new object[0]);
				if (flag)
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Or));
				}
				else
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				m_td.Methods.Add(methodDefinition2);
			}
			else
			{
				Weaver.DLog(m_td, "    Syncvars exist", new object[0]);
				var instruction = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse, instruction));
				foreach (var fieldDefinition in m_SyncVars)
				{
					Weaver.DLog(m_td, $"    For {fieldDefinition.Name}", new object[0]);
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldDefinition));
					var writeFunc = Weaver.GetWriteFunc(fieldDefinition.FieldType);
					if (writeFunc == null)
					{
						Weaver.fail = true;
						Log.Error(string.Concat(new object[]
						{
							"GenerateSerialization for ",
							m_td.Name,
							" unknown type [",
							fieldDefinition.FieldType,
							"]. UNet [SyncVar] member variables must be basic types."
						}));
						return;
					}

					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, writeFunc));
				}

				Weaver.DLog(m_td, $"    Finish foreach 1", new object[0]);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				ilprocessor.Append(instruction);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_0));
				var num = Weaver.GetSyncVarStart(m_td.BaseType.FullName);
				foreach (var fieldDefinition2 in m_SyncVars)
				{
					Weaver.DLog(m_td, $"    For {fieldDefinition2.Name}", new object[0]);
					var instruction2 = ilprocessor.Create(OpCodes.Nop);
					Weaver.DLog(m_td, $"    Got instruction2", new object[0]);
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
					Weaver.DLog(m_td, $"    call dirtbits reference", new object[0]);
					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.NetworkBehaviourDirtyBitsReference));
					Weaver.DLog(m_td, $"    finish call dirtbits reference", new object[0]);
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, 1 << num));
					ilprocessor.Append(ilprocessor.Create(OpCodes.And));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse, instruction2));
					Weaver.DLog(m_td, $"    writing dirtycheck", new object[0]);
					WriteDirtyCheck(ilprocessor, true);
					Weaver.DLog(m_td, $"    done writing dirtycheck", new object[0]);
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldDefinition2));
					Weaver.DLog(m_td, $"    Getting writeFunc2", new object[0]);
					var writeFunc2 = Weaver.GetWriteFunc(fieldDefinition2.FieldType);
					Weaver.DLog(m_td, $"    Got writeFunc2", new object[0]);
					if (writeFunc2 == null)
					{
						Log.Error(string.Concat(new object[]
						{
							"GenerateSerialization for ",
							m_td.Name,
							" unknown type [",
							fieldDefinition2.FieldType,
							"]. UNet [SyncVar] member variables must be basic types."
						}));
						Weaver.fail = true;
						return;
					}

					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, writeFunc2));
					ilprocessor.Append(instruction2);
					num++;
				}

				Weaver.DLog(m_td, $"    Finish foreach 2", new object[0]);
				WriteDirtyCheck(ilprocessor, false);
				if (Weaver.generateLogErrors)
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldstr, "Injected Serialize " + m_td.Name));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.logErrorReference));
				}

				if (flag)
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Or));
				}
				else
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				m_td.Methods.Add(methodDefinition2);
				Weaver.DLog(m_td, $"    Finish", new object[0]);
			}

			Weaver.DLog(m_td, $"  Finish", new object[0]);
		}

		private static void WriteDirtyCheck(ILProcessor serWorker, bool reset)
		{
			var instruction = serWorker.Create(OpCodes.Nop);
			serWorker.Append(serWorker.Create(OpCodes.Ldloc_0));
			serWorker.Append(serWorker.Create(OpCodes.Brtrue, instruction));
			serWorker.Append(serWorker.Create(OpCodes.Ldarg_1));
			serWorker.Append(serWorker.Create(OpCodes.Ldarg_0));
			serWorker.Append(serWorker.Create(OpCodes.Call, Weaver.NetworkBehaviourDirtyBitsReference));
			serWorker.Append(serWorker.Create(OpCodes.Callvirt, Weaver.NetworkWriterWritePacked32));
			if (reset)
			{
				serWorker.Append(serWorker.Create(OpCodes.Ldc_I4_1));
				serWorker.Append(serWorker.Create(OpCodes.Stloc_0));
			}

			serWorker.Append(instruction);
		}

		private static int GetChannelId(FieldDefinition field)
		{
			var result = 0;
			foreach (var customAttribute in field.CustomAttributes)
			{
				if (customAttribute.AttributeType.FullName == Weaver.SyncVarType.FullName)
				{
					foreach (var customAttributeNamedArgument in customAttribute.Fields)
					{
						if (customAttributeNamedArgument.Name == "channel")
						{
							result = (int)customAttributeNamedArgument.Argument.Value;
							break;
						}
					}
				}
			}

			return result;
		}

		private bool CheckForHookFunction(FieldDefinition syncVar, out MethodDefinition foundMethod)
		{
			foundMethod = null;
			foreach (var customAttribute in syncVar.CustomAttributes)
			{
				if (customAttribute.AttributeType.FullName == Weaver.SyncVarType.FullName)
				{
					foreach (var customAttributeNamedArgument in customAttribute.Fields)
					{
						if (customAttributeNamedArgument.Name == "hook")
						{
							var text = customAttributeNamedArgument.Argument.Value as string;
							foreach (var methodDefinition in m_td.Methods)
							{
								if (methodDefinition.Name == text)
								{
									if (methodDefinition.Parameters.Count != 1)
									{
										Log.Error("SyncVar Hook function " + text + " must have one argument " + m_td.Name);
										Weaver.fail = true;
										return false;
									}

									if (methodDefinition.Parameters[0].ParameterType != syncVar.FieldType)
									{
										Log.Error("SyncVar Hook function " + text + " has wrong type signature for " + m_td.Name);
										Weaver.fail = true;
										return false;
									}

									foundMethod = methodDefinition;
									return true;
								}
							}

							Log.Error("SyncVar Hook function " + text + " not found for " + m_td.Name);
							Weaver.fail = true;
							return false;
						}
					}
				}
			}

			return true;
		}

		private void GenerateNetworkChannelSetting(int channel)
		{
			var methodDefinition = new MethodDefinition("GetNetworkChannel", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, Weaver.int32Type);
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, channel));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			m_td.Methods.Add(methodDefinition);
		}

		private void GenerateNetworkIntervalSetting(float interval)
		{
			var methodDefinition = new MethodDefinition("GetNetworkSendInterval", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, Weaver.singleType);
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_R4, interval));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			m_td.Methods.Add(methodDefinition);
		}

		private void GenerateNetworkSettings()
		{
			foreach (var customAttribute in m_td.CustomAttributes)
			{
				if (customAttribute.AttributeType.FullName == Weaver.NetworkSettingsType.FullName)
				{
					foreach (var customAttributeNamedArgument in customAttribute.Fields)
					{
						if (customAttributeNamedArgument.Name == "channel")
						{
							if ((int)customAttributeNamedArgument.Argument.Value == 0)
							{
								continue;
							}

							if (HasMethod("GetNetworkChannel"))
							{
								Log.Error("GetNetworkChannel, is already implemented, please make sure you either use NetworkSettings or GetNetworkChannel");
								Weaver.fail = true;
								return;
							}

							m_QosChannel = (int)customAttributeNamedArgument.Argument.Value;
							GenerateNetworkChannelSetting(m_QosChannel);
						}

						if (customAttributeNamedArgument.Name == "sendInterval")
						{
							if (Math.Abs((float)customAttributeNamedArgument.Argument.Value - 0.1f) > 1E-05f)
							{
								if (HasMethod("GetNetworkSendInterval"))
								{
									Log.Error("GetNetworkSendInterval, is already implemented, please make sure you either use NetworkSettings or GetNetworkSendInterval");
									Weaver.fail = true;
									return;
								}

								GenerateNetworkIntervalSetting((float)customAttributeNamedArgument.Argument.Value);
							}
						}
					}
				}
			}
		}

		private void GeneratePreStartClient()
		{
			m_NetIdFieldCounter = 0;
			MethodDefinition methodDefinition = null;
			ILProcessor ilprocessor = null;
			foreach (var methodDefinition2 in m_td.Methods)
			{
				if (methodDefinition2.Name == "PreStartClient")
				{
					return;
				}
			}

			foreach (var fieldDefinition in m_SyncVars)
			{
				if (fieldDefinition.FieldType.FullName == Weaver.gameObjectType.FullName)
				{
					if (methodDefinition == null)
					{
						methodDefinition = new MethodDefinition("PreStartClient", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, Weaver.voidType);
						ilprocessor = methodDefinition.Body.GetILProcessor();
					}

					var field = m_SyncVarNetIds[m_NetIdFieldCounter];
					m_NetIdFieldCounter++;
					var instruction = ilprocessor.Create(OpCodes.Nop);
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldflda, field));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.NetworkInstanceIsEmpty));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Brtrue, instruction));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, field));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.FindLocalObjectReference));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Stfld, fieldDefinition));
					ilprocessor.Append(instruction);
				}
			}

			if (methodDefinition != null)
			{
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				m_td.Methods.Add(methodDefinition);
			}
		}

		private void GenerateDeSerialization()
		{
			Weaver.DLog(m_td, "  GenerateDeSerialization", new object[0]);
			m_NetIdFieldCounter = 0;
			foreach (var methodDefinition in m_td.Methods)
			{
				if (methodDefinition.Name == "OnDeserialize")
				{
					return;
				}
			}

			var methodDefinition2 = new MethodDefinition("OnDeserialize", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, Weaver.voidType);
			methodDefinition2.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkReaderType)));
			methodDefinition2.Parameters.Add(new ParameterDefinition("initialState", ParameterAttributes.None, Weaver.boolType));
			var ilprocessor = methodDefinition2.Body.GetILProcessor();
			if (m_td.BaseType.FullName != Weaver.NetworkBehaviourType.FullName)
			{
				var methodReference = Weaver.ResolveMethod(m_td.BaseType, "OnDeserialize");
				if (methodReference != null)
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_2));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, methodReference));
				}
			}

			if (m_SyncVars.Count == 0)
			{
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				m_td.Methods.Add(methodDefinition2);
			}
			else
			{
				var instruction = ilprocessor.Create(OpCodes.Nop);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse, instruction));
				foreach (var fieldDefinition in m_SyncVars)
				{
					var readByReferenceFunc = Weaver.GetReadByReferenceFunc(fieldDefinition.FieldType);
					if (readByReferenceFunc != null)
					{
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldDefinition));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Call, readByReferenceFunc));
					}
					else
					{
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
						if (fieldDefinition.FieldType.FullName == Weaver.gameObjectType.FullName)
						{
							var field = m_SyncVarNetIds[m_NetIdFieldCounter];
							m_NetIdFieldCounter++;
							ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkReaderReadNetworkInstanceId));
							ilprocessor.Append(ilprocessor.Create(OpCodes.Stfld, field));
						}
						else
						{
							var readFunc = Weaver.GetReadFunc(fieldDefinition.FieldType);
							if (readFunc == null)
							{
								Log.Error(string.Concat(new object[]
								{
									"GenerateDeSerialization for ",
									m_td.Name,
									" unknown type [",
									fieldDefinition.FieldType,
									"]. UNet [SyncVar] member variables must be basic types."
								}));
								Weaver.fail = true;
								return;
							}

							ilprocessor.Append(ilprocessor.Create(OpCodes.Call, readFunc));
							ilprocessor.Append(ilprocessor.Create(OpCodes.Stfld, fieldDefinition));
						}
					}
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				ilprocessor.Append(instruction);
				methodDefinition2.Body.InitLocals = true;
				var item = new VariableDefinition(Weaver.int32Type);
				methodDefinition2.Body.Variables.Add(item);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkReaderReadPacked32));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_0));
				var num = Weaver.GetSyncVarStart(m_td.BaseType.FullName);
				foreach (var fieldDefinition2 in m_SyncVars)
				{
					var instruction2 = ilprocessor.Create(OpCodes.Nop);
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, 1 << num));
					ilprocessor.Append(ilprocessor.Create(OpCodes.And));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse, instruction2));
					var readByReferenceFunc2 = Weaver.GetReadByReferenceFunc(fieldDefinition2.FieldType);
					if (readByReferenceFunc2 != null)
					{
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldDefinition2));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Call, readByReferenceFunc2));
					}
					else
					{
						var readFunc2 = Weaver.GetReadFunc(fieldDefinition2.FieldType);
						if (readFunc2 == null)
						{
							Log.Error(string.Concat(new object[]
							{
								"GenerateDeSerialization for ",
								m_td.Name,
								" unknown type [",
								fieldDefinition2.FieldType,
								"]. UNet [SyncVar] member variables must be basic types."
							}));
							Weaver.fail = true;
							return;
						}

						if (!CheckForHookFunction(fieldDefinition2, out var methodDefinition3))
						{
							return;
						}

						if (methodDefinition3 == null)
						{
							ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
							ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
							ilprocessor.Append(ilprocessor.Create(OpCodes.Call, readFunc2));
							ilprocessor.Append(ilprocessor.Create(OpCodes.Stfld, fieldDefinition2));
						}
						else
						{
							ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
							ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
							ilprocessor.Append(ilprocessor.Create(OpCodes.Call, readFunc2));
							ilprocessor.Append(ilprocessor.Create(OpCodes.Call, methodDefinition3));
						}
					}

					ilprocessor.Append(instruction2);
					num++;
				}

				if (Weaver.generateLogErrors)
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldstr, "Injected Deserialize " + m_td.Name));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.logErrorReference));
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				m_td.Methods.Add(methodDefinition2);
			}
		}

		private bool ProcessNetworkReaderParameters(MethodDefinition md, ILProcessor worker, bool skipFirst)
		{
			var num = 0;
			foreach (var parameterDefinition in md.Parameters)
			{
				if (num++ != 0 || !skipFirst)
				{
					var readFunc = Weaver.GetReadFunc(parameterDefinition.ParameterType);
					if (readFunc == null)
					{
						Log.Error(string.Concat(new object[]
						{
							"ProcessNetworkReaderParameters for ",
							m_td.Name,
							":",
							md.Name,
							" type ",
							parameterDefinition.ParameterType,
							" not supported"
						}));
						Weaver.fail = true;
						return false;
					}

					worker.Append(worker.Create(OpCodes.Ldarg_1));
					worker.Append(worker.Create(OpCodes.Call, readFunc));
					if (parameterDefinition.ParameterType.FullName == Weaver.singleType.FullName)
					{
						worker.Append(worker.Create(OpCodes.Conv_R4));
					}
					else if (parameterDefinition.ParameterType.FullName == Weaver.doubleType.FullName)
					{
						worker.Append(worker.Create(OpCodes.Conv_R8));
					}
				}
			}

			return true;
		}

		private MethodDefinition ProcessCommandInvoke(MethodDefinition md)
		{
			var methodDefinition = new MethodDefinition("InvokeCmd" + md.Name, MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			var label = ilprocessor.Create(OpCodes.Nop);
			WriteServerActiveCheck(ilprocessor, md.Name, label, "Command");
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Castclass, m_td));
			MethodDefinition result;
			if (!ProcessNetworkReaderParameters(md, ilprocessor, false))
			{
				result = null;
			}
			else
			{
				ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, md));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				AddInvokeParameters(methodDefinition.Parameters);
				result = methodDefinition;
			}

			return result;
		}

		private static void AddInvokeParameters(ICollection<ParameterDefinition> collection)
		{
			collection.Add(new ParameterDefinition("obj", ParameterAttributes.None, Weaver.NetworkBehaviourType2));
			collection.Add(new ParameterDefinition("reader", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkReaderType)));
		}

		private MethodDefinition ProcessCommandCall(MethodDefinition md, CustomAttribute ca)
		{
			var methodDefinition = new MethodDefinition("Call" + md.Name, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig, Weaver.voidType);
			foreach (var parameterDefinition in md.Parameters)
			{
				methodDefinition.Parameters.Add(new ParameterDefinition(parameterDefinition.Name, ParameterAttributes.None, parameterDefinition.ParameterType));
			}

			var ilprocessor = methodDefinition.Body.GetILProcessor();
			var label = ilprocessor.Create(OpCodes.Nop);
			WriteSetupLocals(ilprocessor);
			if (Weaver.generateLogErrors)
			{
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldstr, "Call Command function " + md.Name));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.logErrorReference));
			}

			WriteClientActiveCheck(ilprocessor, md.Name, label, "Command function");
			var instruction = ilprocessor.Create(OpCodes.Nop);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.UBehaviourIsServer));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse, instruction));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			for (var i = 0; i < md.Parameters.Count; i++)
			{
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg, i + 1));
			}

			ilprocessor.Append(ilprocessor.Create(OpCodes.Call, md));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			ilprocessor.Append(instruction);
			WriteCreateWriter(ilprocessor);
			WriteMessageSize(ilprocessor);
			WriteMessageId(ilprocessor, 5);
			var fieldDefinition = new FieldDefinition("kCmd" + md.Name, FieldAttributes.Private | FieldAttributes.Static, Weaver.int32Type);
			m_td.Fields.Add(fieldDefinition);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldsfld, fieldDefinition));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkWriterWritePacked32));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.getComponentReference));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.getUNetIdReference));
			var writeFunc = Weaver.GetWriteFunc(Weaver.NetworkInstanceIdType);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, writeFunc));
			MethodDefinition result;
			if (!WriteArguments(ilprocessor, md, "Command", false))
			{
				result = null;
			}
			else
			{
				var value = 0;
				foreach (var customAttributeNamedArgument in ca.Fields)
				{
					if (customAttributeNamedArgument.Name == "channel")
					{
						value = (int)customAttributeNamedArgument.Argument.Value;
					}
				}

				var text = md.Name;
				var num = text.IndexOf("InvokeCmd");
				if (num > -1)
				{
					text = text.Substring("InvokeCmd".Length);
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, value));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldstr, text));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.sendCommandInternal));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				result = methodDefinition;
			}

			return result;
		}

		private MethodDefinition ProcessTargetRpcInvoke(MethodDefinition md)
		{
			var methodDefinition = new MethodDefinition("InvokeRpc" + md.Name, MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			var label = ilprocessor.Create(OpCodes.Nop);
			WriteClientActiveCheck(ilprocessor, md.Name, label, "TargetRPC");
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Castclass, m_td));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.ReadyConnectionReference));
			MethodDefinition result;
			if (!ProcessNetworkReaderParameters(md, ilprocessor, true))
			{
				result = null;
			}
			else
			{
				ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, md));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				AddInvokeParameters(methodDefinition.Parameters);
				result = methodDefinition;
			}

			return result;
		}

		private MethodDefinition ProcessRpcInvoke(MethodDefinition md)
		{
			var methodDefinition = new MethodDefinition("InvokeRpc" + md.Name, MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			var label = ilprocessor.Create(OpCodes.Nop);
			WriteClientActiveCheck(ilprocessor, md.Name, label, "RPC");
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Castclass, m_td));
			MethodDefinition result;
			if (!ProcessNetworkReaderParameters(md, ilprocessor, false))
			{
				result = null;
			}
			else
			{
				ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, md));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				AddInvokeParameters(methodDefinition.Parameters);
				result = methodDefinition;
			}

			return result;
		}

		private MethodDefinition ProcessTargetRpcCall(MethodDefinition md, CustomAttribute ca)
		{
			var methodDefinition = new MethodDefinition("Call" + md.Name, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig, Weaver.voidType);
			foreach (var parameterDefinition in md.Parameters)
			{
				methodDefinition.Parameters.Add(new ParameterDefinition(parameterDefinition.Name, ParameterAttributes.None, parameterDefinition.ParameterType));
			}

			var ilprocessor = methodDefinition.Body.GetILProcessor();
			var label = ilprocessor.Create(OpCodes.Nop);
			WriteSetupLocals(ilprocessor);
			WriteServerActiveCheck(ilprocessor, md.Name, label, "TargetRPC Function");
			var instruction = ilprocessor.Create(OpCodes.Nop);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Isinst, Weaver.ULocalConnectionToServerType));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse, instruction));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldstr, string.Format("TargetRPC Function {0} called on connection to server", md.Name)));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.logErrorReference));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			ilprocessor.Append(instruction);
			WriteCreateWriter(ilprocessor);
			WriteMessageSize(ilprocessor);
			WriteMessageId(ilprocessor, 2);
			var fieldDefinition = new FieldDefinition("kTargetRpc" + md.Name, FieldAttributes.Private | FieldAttributes.Static, Weaver.int32Type);
			m_td.Fields.Add(fieldDefinition);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldsfld, fieldDefinition));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkWriterWritePacked32));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.getComponentReference));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.getUNetIdReference));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkWriterWriteNetworkInstanceId));
			MethodDefinition result;
			if (!WriteArguments(ilprocessor, md, "TargetRPC", true))
			{
				result = null;
			}
			else
			{
				var value = 0;
				foreach (var customAttributeNamedArgument in ca.Fields)
				{
					if (customAttributeNamedArgument.Name == "channel")
					{
						value = (int)customAttributeNamedArgument.Argument.Value;
					}
				}

				var text = md.Name;
				var num = text.IndexOf("InvokeTargetRpc");
				if (num > -1)
				{
					text = text.Substring("InvokeTargetRpc".Length);
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, value));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldstr, text));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.sendTargetRpcInternal));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				result = methodDefinition;
			}

			return result;
		}

		private MethodDefinition ProcessRpcCall(MethodDefinition md, CustomAttribute ca)
		{
			var methodDefinition = new MethodDefinition("Call" + md.Name, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig, Weaver.voidType);
			foreach (var parameterDefinition in md.Parameters)
			{
				methodDefinition.Parameters.Add(new ParameterDefinition(parameterDefinition.Name, ParameterAttributes.None, parameterDefinition.ParameterType));
			}

			var ilprocessor = methodDefinition.Body.GetILProcessor();
			var label = ilprocessor.Create(OpCodes.Nop);
			WriteSetupLocals(ilprocessor);
			WriteServerActiveCheck(ilprocessor, md.Name, label, "RPC Function");
			WriteCreateWriter(ilprocessor);
			WriteMessageSize(ilprocessor);
			WriteMessageId(ilprocessor, 2);
			var fieldDefinition = new FieldDefinition("kRpc" + md.Name, FieldAttributes.Private | FieldAttributes.Static, Weaver.int32Type);
			m_td.Fields.Add(fieldDefinition);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldsfld, fieldDefinition));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkWriterWritePacked32));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.getComponentReference));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.getUNetIdReference));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkWriterWriteNetworkInstanceId));
			MethodDefinition result;
			if (!WriteArguments(ilprocessor, md, "RPC", false))
			{
				result = null;
			}
			else
			{
				var value = 0;
				foreach (var customAttributeNamedArgument in ca.Fields)
				{
					if (customAttributeNamedArgument.Name == "channel")
					{
						value = (int)customAttributeNamedArgument.Argument.Value;
					}
				}

				var text = md.Name;
				var num = text.IndexOf("InvokeRpc");
				if (num > -1)
				{
					text = text.Substring("InvokeRpc".Length);
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, value));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldstr, text));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.sendRpcInternal));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				result = methodDefinition;
			}

			return result;
		}

		private bool ProcessMethodsValidateFunction(MethodReference md, CustomAttribute ca, string actionType)
		{
			bool result;
			if (md.ReturnType.FullName == Weaver.IEnumeratorType.FullName)
			{
				Log.Error(string.Concat(new string[]
				{
					actionType,
					" function [",
					m_td.FullName,
					":",
					md.Name,
					"] cannot be a coroutine"
				}));
				Weaver.fail = true;
				result = false;
			}
			else if (md.ReturnType.FullName != Weaver.voidType.FullName)
			{
				Log.Error(string.Concat(new string[]
				{
					actionType,
					" function [",
					m_td.FullName,
					":",
					md.Name,
					"] must have a void return type."
				}));
				Weaver.fail = true;
				result = false;
			}
			else if (md.HasGenericParameters)
			{
				Log.Error(string.Concat(new string[]
				{
					actionType,
					" [",
					m_td.FullName,
					":",
					md.Name,
					"] cannot have generic parameters"
				}));
				Weaver.fail = true;
				result = false;
			}
			else
			{
				result = true;
			}

			return result;
		}

		private bool ProcessMethodsValidateParameters(MethodReference md, CustomAttribute ca, string actionType)
		{
			var i = 0;
			while (i < md.Parameters.Count)
			{
				var parameterDefinition = md.Parameters[i];
				bool result;
				if (parameterDefinition.IsOut)
				{
					Log.Error(string.Concat(new string[]
					{
						actionType,
						" function [",
						m_td.FullName,
						":",
						md.Name,
						"] cannot have out parameters"
					}));
					Weaver.fail = true;
					result = false;
				}
				else if (parameterDefinition.IsOptional)
				{
					Log.Error(string.Concat(new string[]
					{
						actionType,
						"function [",
						m_td.FullName,
						":",
						md.Name,
						"] cannot have optional parameters"
					}));
					Weaver.fail = true;
					result = false;
				}
				else if (parameterDefinition.ParameterType.Resolve().IsAbstract)
				{
					Log.Error(string.Concat(new string[]
					{
						actionType,
						" function [",
						m_td.FullName,
						":",
						md.Name,
						"] cannot have abstract parameters"
					}));
					Weaver.fail = true;
					result = false;
				}
				else if (parameterDefinition.ParameterType.IsByReference)
				{
					Log.Error(string.Concat(new string[]
					{
						actionType,
						" function [",
						m_td.FullName,
						":",
						md.Name,
						"] cannot have ref parameters"
					}));
					Weaver.fail = true;
					result = false;
				}
				else
				{
					if (!(parameterDefinition.ParameterType.FullName == Weaver.NetworkConnectionType.FullName) || (ca.AttributeType.FullName == Weaver.TargetRpcType.FullName && i == 0))
					{
						if (Weaver.IsDerivedFrom(parameterDefinition.ParameterType.Resolve(), Weaver.ComponentType))
						{
							if (parameterDefinition.ParameterType.FullName != Weaver.NetworkIdentityType.FullName)
							{
								Log.Error(string.Concat(new string[]
								{
									actionType,
									" function [",
									m_td.FullName,
									":",
									md.Name,
									"] parameter [",
									parameterDefinition.Name,
									"] is of the type [",
									parameterDefinition.ParameterType.Name,
									"] which is a Component. You cannot pass a Component to a remote call. Try passing data from within the component."
								}));
								Weaver.fail = true;
								return false;
							}
						}

						i++;
						continue;
					}

					Log.Error(string.Concat(new string[]
					{
						actionType,
						" [",
						m_td.FullName,
						":",
						md.Name,
						"] cannot use a NetworkConnection as a parameter. To access a player object's connection on the server use connectionToClient"
					}));
					Log.Error("Name: " + ca.AttributeType.FullName + " parameter: " + md.Parameters[0].ParameterType.FullName);
					Weaver.fail = true;
					result = false;
				}

				return result;
			}

			return true;
		}

		private bool ProcessMethodsValidateCommand(MethodDefinition md, CustomAttribute ca)
		{
			bool result;
			if (md.Name.Length > 2 && md.Name.Substring(0, 3) != "Cmd")
			{
				Log.Error(string.Concat(new string[]
				{
					"Command function [",
					m_td.FullName,
					":",
					md.Name,
					"] doesnt have 'Cmd' prefix"
				}));
				Weaver.fail = true;
				result = false;
			}
			else if (md.IsStatic)
			{
				Log.Error(string.Concat(new string[]
				{
					"Command function [",
					m_td.FullName,
					":",
					md.Name,
					"] cant be a static method"
				}));
				Weaver.fail = true;
				result = false;
			}
			else
			{
				result = (ProcessMethodsValidateFunction(md, ca, "Command") && ProcessMethodsValidateParameters(md, ca, "Command"));
			}

			return result;
		}

		private bool ProcessMethodsValidateTargetRpc(MethodDefinition md, CustomAttribute ca)
		{
			var length = "Target".Length;
			bool result;
			if (md.Name.Length > length && md.Name.Substring(0, length) != "Target")
			{
				Log.Error(string.Concat(new string[]
				{
					"Target Rpc function [",
					m_td.FullName,
					":",
					md.Name,
					"] doesnt have 'Target' prefix"
				}));
				Weaver.fail = true;
				result = false;
			}
			else if (md.IsStatic)
			{
				Log.Error(string.Concat(new string[]
				{
					"TargetRpc function [",
					m_td.FullName,
					":",
					md.Name,
					"] cant be a static method"
				}));
				Weaver.fail = true;
				result = false;
			}
			else if (!ProcessMethodsValidateFunction(md, ca, "Target Rpc"))
			{
				result = false;
			}
			else if (md.Parameters.Count < 1)
			{
				Log.Error(string.Concat(new string[]
				{
					"Target Rpc function [",
					m_td.FullName,
					":",
					md.Name,
					"] must have a NetworkConnection as the first parameter"
				}));
				Weaver.fail = true;
				result = false;
			}
			else if (md.Parameters[0].ParameterType.FullName != Weaver.NetworkConnectionType.FullName)
			{
				Log.Error(string.Concat(new string[]
				{
					"Target Rpc function [",
					m_td.FullName,
					":",
					md.Name,
					"] first parameter must be a NetworkConnection"
				}));
				Weaver.fail = true;
				result = false;
			}
			else
			{
				result = ProcessMethodsValidateParameters(md, ca, "Target Rpc");
			}

			return result;
		}

		private bool ProcessMethodsValidateRpc(MethodDefinition md, CustomAttribute ca)
		{
			bool result;
			if (md.Name.Length > 2 && md.Name.Substring(0, 3) != "Rpc")
			{
				Log.Error(string.Concat(new string[]
				{
					"Rpc function [",
					m_td.FullName,
					":",
					md.Name,
					"] doesnt have 'Rpc' prefix"
				}));
				Weaver.fail = true;
				result = false;
			}
			else if (md.IsStatic)
			{
				Log.Error(string.Concat(new string[]
				{
					"ClientRpc function [",
					m_td.FullName,
					":",
					md.Name,
					"] cant be a static method"
				}));
				Weaver.fail = true;
				result = false;
			}
			else
			{
				result = (ProcessMethodsValidateFunction(md, ca, "Rpc") && ProcessMethodsValidateParameters(md, ca, "Rpc"));
			}

			return result;
		}

		private void ProcessMethods()
		{
			var hashSet = new HashSet<string>();
			foreach (var methodDefinition in m_td.Methods)
			{
				Weaver.ResetRecursionCount();
				foreach (var customAttribute in methodDefinition.CustomAttributes)
				{
					if (customAttribute.AttributeType.FullName == Weaver.CommandType.FullName)
					{
						if (!ProcessMethodsValidateCommand(methodDefinition, customAttribute))
						{
							return;
						}

						if (hashSet.Contains(methodDefinition.Name))
						{
							Log.Error(string.Concat(new string[]
							{
								"Duplicate Command name [",
								m_td.FullName,
								":",
								methodDefinition.Name,
								"]"
							}));
							Weaver.fail = true;
							return;
						}

						hashSet.Add(methodDefinition.Name);
						m_Cmds.Add(methodDefinition);
						var methodDefinition2 = ProcessCommandInvoke(methodDefinition);
						if (methodDefinition2 != null)
						{
							m_CmdInvocationFuncs.Add(methodDefinition2);
						}

						var methodDefinition3 = ProcessCommandCall(methodDefinition, customAttribute);
						if (methodDefinition3 != null)
						{
							m_CmdCallFuncs.Add(methodDefinition3);
							Weaver.lists.replacedMethods.Add(methodDefinition);
							Weaver.lists.replacementMethods.Add(methodDefinition3);
						}

						break;
					}
					else if (customAttribute.AttributeType.FullName == Weaver.TargetRpcType.FullName)
					{
						if (!ProcessMethodsValidateTargetRpc(methodDefinition, customAttribute))
						{
							return;
						}

						if (hashSet.Contains(methodDefinition.Name))
						{
							Log.Error(string.Concat(new string[]
							{
								"Duplicate Target Rpc name [",
								m_td.FullName,
								":",
								methodDefinition.Name,
								"]"
							}));
							Weaver.fail = true;
							return;
						}

						hashSet.Add(methodDefinition.Name);
						m_TargetRpcs.Add(methodDefinition);
						var methodDefinition4 = ProcessTargetRpcInvoke(methodDefinition);
						if (methodDefinition4 != null)
						{
							m_TargetRpcInvocationFuncs.Add(methodDefinition4);
						}

						var methodDefinition5 = ProcessTargetRpcCall(methodDefinition, customAttribute);
						if (methodDefinition5 != null)
						{
							m_TargetRpcCallFuncs.Add(methodDefinition5);
							Weaver.lists.replacedMethods.Add(methodDefinition);
							Weaver.lists.replacementMethods.Add(methodDefinition5);
						}

						break;
					}
					else if (customAttribute.AttributeType.FullName == Weaver.ClientRpcType.FullName)
					{
						if (!ProcessMethodsValidateRpc(methodDefinition, customAttribute))
						{
							return;
						}

						if (hashSet.Contains(methodDefinition.Name))
						{
							Log.Error(string.Concat(new string[]
							{
								"Duplicate ClientRpc name [",
								m_td.FullName,
								":",
								methodDefinition.Name,
								"]"
							}));
							Weaver.fail = true;
							return;
						}

						hashSet.Add(methodDefinition.Name);
						m_Rpcs.Add(methodDefinition);
						var methodDefinition6 = ProcessRpcInvoke(methodDefinition);
						if (methodDefinition6 != null)
						{
							m_RpcInvocationFuncs.Add(methodDefinition6);
						}

						var methodDefinition7 = ProcessRpcCall(methodDefinition, customAttribute);
						if (methodDefinition7 != null)
						{
							m_RpcCallFuncs.Add(methodDefinition7);
							Weaver.lists.replacedMethods.Add(methodDefinition);
							Weaver.lists.replacementMethods.Add(methodDefinition7);
						}

						break;
					}
				}
			}

			foreach (var item in m_CmdInvocationFuncs)
			{
				m_td.Methods.Add(item);
			}

			foreach (var item2 in m_CmdCallFuncs)
			{
				m_td.Methods.Add(item2);
			}

			foreach (var item3 in m_RpcInvocationFuncs)
			{
				m_td.Methods.Add(item3);
			}

			foreach (var item4 in m_TargetRpcInvocationFuncs)
			{
				m_td.Methods.Add(item4);
			}

			foreach (var item5 in m_RpcCallFuncs)
			{
				m_td.Methods.Add(item5);
			}

			foreach (var item6 in m_TargetRpcCallFuncs)
			{
				m_td.Methods.Add(item6);
			}
		}

		private MethodDefinition ProcessEventInvoke(EventDefinition ed)
		{
			FieldDefinition fieldDefinition = null;
			foreach (var fieldDefinition2 in m_td.Fields)
			{
				if (fieldDefinition2.FullName == ed.FullName)
				{
					fieldDefinition = fieldDefinition2;
					break;
				}
			}

			MethodDefinition result;
			if (fieldDefinition == null)
			{
				Weaver.DLog(m_td, "ERROR: no event field?!", new object[0]);
				Weaver.fail = true;
				result = null;
			}
			else
			{
				var methodDefinition = new MethodDefinition("InvokeSyncEvent" + ed.Name, MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
				var ilprocessor = methodDefinition.Body.GetILProcessor();
				var label = ilprocessor.Create(OpCodes.Nop);
				var instruction = ilprocessor.Create(OpCodes.Nop);
				WriteClientActiveCheck(ilprocessor, ed.Name, label, "Event");
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Castclass, m_td));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldDefinition));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brtrue, instruction));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				ilprocessor.Append(instruction);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Castclass, m_td));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldDefinition));
				var methodReference = Weaver.ResolveMethod(fieldDefinition.FieldType, "Invoke");
				if (!ProcessNetworkReaderParameters(methodReference.Resolve(), ilprocessor, false))
				{
					result = null;
				}
				else
				{
					ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, methodReference));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
					AddInvokeParameters(methodDefinition.Parameters);
					result = methodDefinition;
				}
			}

			return result;
		}

		private MethodDefinition ProcessEventCall(EventDefinition ed, CustomAttribute ca)
		{
			var methodReference = Weaver.ResolveMethod(ed.EventType, "Invoke");
			var methodDefinition = new MethodDefinition("Call" + ed.Name, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig, Weaver.voidType);
			foreach (var parameterDefinition in methodReference.Parameters)
			{
				methodDefinition.Parameters.Add(new ParameterDefinition(parameterDefinition.Name, ParameterAttributes.None, parameterDefinition.ParameterType));
			}

			var ilprocessor = methodDefinition.Body.GetILProcessor();
			var label = ilprocessor.Create(OpCodes.Nop);
			WriteSetupLocals(ilprocessor);
			WriteServerActiveCheck(ilprocessor, ed.Name, label, "Event");
			WriteCreateWriter(ilprocessor);
			WriteMessageSize(ilprocessor);
			WriteMessageId(ilprocessor, 7);
			var fieldDefinition = new FieldDefinition("kEvent" + ed.Name, FieldAttributes.Private | FieldAttributes.Static, Weaver.int32Type);
			m_td.Fields.Add(fieldDefinition);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldsfld, fieldDefinition));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkWriterWritePacked32));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.getComponentReference));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.getUNetIdReference));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkWriterWriteNetworkInstanceId));
			MethodDefinition result;
			if (!WriteArguments(ilprocessor, methodReference.Resolve(), "SyncEvent", false))
			{
				result = null;
			}
			else
			{
				var value = 0;
				foreach (var customAttributeNamedArgument in ca.Fields)
				{
					if (customAttributeNamedArgument.Name == "channel")
					{
						value = (int)customAttributeNamedArgument.Argument.Value;
					}
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, value));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldstr, ed.Name));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.sendEventInternal));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				result = methodDefinition;
			}

			return result;
		}

		private void ProcessEvents()
		{
			foreach (var eventDefinition in m_td.Events)
			{
				foreach (var customAttribute in eventDefinition.CustomAttributes)
				{
					if (customAttribute.AttributeType.FullName == Weaver.SyncEventType.FullName)
					{
						if (eventDefinition.Name.Length > 4 && eventDefinition.Name.Substring(0, 5) != "Event")
						{
							Log.Error(string.Concat(new string[]
							{
								"Event  [",
								m_td.FullName,
								":",
								eventDefinition.FullName,
								"] doesnt have 'Event' prefix"
							}));
							Weaver.fail = true;
							return;
						}

						if (eventDefinition.EventType.Resolve().HasGenericParameters)
						{
							Log.Error(string.Concat(new string[]
							{
								"Event  [",
								m_td.FullName,
								":",
								eventDefinition.FullName,
								"] cannot have generic parameters"
							}));
							Weaver.fail = true;
							return;
						}

						m_Events.Add(eventDefinition);
						var methodDefinition = ProcessEventInvoke(eventDefinition);
						if (methodDefinition == null)
						{
							return;
						}

						m_td.Methods.Add(methodDefinition);
						m_EventInvocationFuncs.Add(methodDefinition);
						Weaver.DLog(m_td, "ProcessEvent " + eventDefinition, new object[0]);
						var methodDefinition2 = ProcessEventCall(eventDefinition, customAttribute);
						m_td.Methods.Add(methodDefinition2);
						Weaver.lists.replacedEvents.Add(eventDefinition);
						Weaver.lists.replacementEvents.Add(methodDefinition2);
						Weaver.DLog(m_td, "  Event: " + eventDefinition.Name, new object[0]);
						break;
					}
				}
			}
		}

		private static MethodDefinition ProcessSyncVarGet(FieldDefinition fd, string originalName)
		{
			var methodDefinition = new MethodDefinition("get_Network" + originalName, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName, fd.FieldType);
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fd));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			methodDefinition.Body.Variables.Add(new VariableDefinition(fd.FieldType));
			methodDefinition.Body.InitLocals = true;
			methodDefinition.SemanticsAttributes = MethodSemanticsAttributes.Getter;
			return methodDefinition;
		}

		private MethodDefinition ProcessSyncVarSet(FieldDefinition fd, string originalName, int dirtyBit, FieldDefinition netFieldId)
		{
			var methodDefinition = new MethodDefinition("set_Network" + originalName, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName, Weaver.voidType);
			var ilprocessor = methodDefinition.Body.GetILProcessor();

			var noOperatorInstruction = ilprocessor.Create(OpCodes.Nop);
			var returnInstruction = ilprocessor.Create(OpCodes.Ret);

			CheckForHookFunction(fd, out var methodDefinition2);
			if (methodDefinition2 != null)
			{

				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.NetworkServerGetLocalClientActive));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse, noOperatorInstruction));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.getSyncVarHookGuard));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brtrue, noOperatorInstruction));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.setSyncVarHookGuard));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, methodDefinition2));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.setSyncVarHookGuard));
				ilprocessor.Append(noOperatorInstruction);
			}

			if (fd.FieldType.FullName == Weaver.gameObjectType.FullName)
			{
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldflda, netFieldId));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.setSyncVarGameObjectReference));
			}
			else
			{
				var genericInstanceMethod = new GenericInstanceMethod(Weaver.setSyncVarReference);
				genericInstanceMethod.GenericArguments.Add(fd.FieldType);

				var index11 = ilprocessor.Create(OpCodes.Ldarg_0);

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.UBehaviourIsServer));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse_S, index11));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldflda, fd));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, dirtyBit));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, genericInstanceMethod));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Pop));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				ilprocessor.Append(index11);
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldflda, fd));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4, dirtyBit));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, genericInstanceMethod));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Brfalse_S, returnInstruction));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
				ilprocessor.Append(ilprocessor.Create(OpCodes.Call, Weaver.NetworkBehaviourClientSendUpdateVars));
			}

			ilprocessor.Append(returnInstruction);
			methodDefinition.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.In, fd.FieldType));
			methodDefinition.SemanticsAttributes = MethodSemanticsAttributes.Setter;
			return methodDefinition;
		}

		private void ProcessSyncVar(FieldDefinition fd, int dirtyBit)
		{
			var name = fd.Name;
			Weaver.lists.replacedFields.Add(fd);
			Weaver.DLog(m_td, $"Found SyncVar {fd.Name} of type {fd.FieldType}", new object[0]);
			FieldDefinition fieldDefinition = null;
			if (fd.FieldType.FullName == Weaver.gameObjectType.FullName)
			{
				fieldDefinition = new FieldDefinition("___" + fd.Name + "NetId", FieldAttributes.Private, Weaver.NetworkInstanceIdType);
				m_SyncVarNetIds.Add(fieldDefinition);
				Weaver.lists.netIdFields.Add(fieldDefinition);
			}

			var methodDefinition = ProcessSyncVarGet(fd, name);
			var methodDefinition2 = ProcessSyncVarSet(fd, name, dirtyBit, fieldDefinition);
			var item = new PropertyDefinition("Network" + name, PropertyAttributes.None, fd.FieldType)
			{
				GetMethod = methodDefinition,
				SetMethod = methodDefinition2
			};
			m_td.Methods.Add(methodDefinition);
			m_td.Methods.Add(methodDefinition2);
			m_td.Properties.Add(item);
			Weaver.lists.replacementProperties.Add(methodDefinition2);
		}

		private static MethodDefinition ProcessSyncListInvoke(FieldDefinition fd)
		{
			var methodDefinition = new MethodDefinition("InvokeSyncList" + fd.Name, MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			var label = ilprocessor.Create(OpCodes.Nop);
			WriteClientActiveCheck(ilprocessor, fd.Name, label, "SyncList");
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Castclass, fd.DeclaringType));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fd));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
			var genericInstanceType = (GenericInstanceType)fd.FieldType.Resolve().BaseType;
			genericInstanceType = (GenericInstanceType)Weaver.scriptDef.MainModule.ImportReference(genericInstanceType);
			var typeReference = genericInstanceType.GenericArguments[0];
			var method = Helpers.MakeHostInstanceGeneric(Weaver.SyncListInitHandleMsg, new TypeReference[]
			{
				typeReference
			});
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, method));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			AddInvokeParameters(methodDefinition.Parameters);
			return methodDefinition;
		}

		private FieldDefinition ProcessSyncList(FieldDefinition fd, int dirtyBit)
		{
			var methodDefinition = ProcessSyncListInvoke(fd);
			m_SyncListInvocationFuncs.Add(methodDefinition);
			return new FieldDefinition("kList" + fd.Name, FieldAttributes.Private | FieldAttributes.Static, Weaver.int32Type);
		}

		private void ProcessSyncVars()
		{
			var num = 0;
			var num2 = Weaver.GetSyncVarStart(m_td.BaseType.FullName);
			m_SyncVarNetIds.Clear();
			var list = new List<FieldDefinition>();
			foreach (var fieldDefinition in m_td.Fields)
			{
				foreach (var customAttribute in fieldDefinition.CustomAttributes)
				{
					if (customAttribute.AttributeType.FullName == Weaver.SyncVarType.FullName)
					{
						var typeDefinition = fieldDefinition.FieldType.Resolve();
						if (Weaver.IsDerivedFrom(typeDefinition, Weaver.NetworkBehaviourType))
						{
							Log.Error("SyncVar [" + fieldDefinition.FullName + "] cannot be derived from NetworkBehaviour.");
							Weaver.fail = true;
							return;
						}

						if (Weaver.IsDerivedFrom(typeDefinition, Weaver.ScriptableObjectType))
						{
							Log.Error("SyncVar [" + fieldDefinition.FullName + "] cannot be derived from ScriptableObject.");
							Weaver.fail = true;
							return;
						}

						if ((ushort)(fieldDefinition.Attributes & FieldAttributes.Static) != 0)
						{
							Log.Error("SyncVar [" + fieldDefinition.FullName + "] cannot be static.");
							Weaver.fail = true;
							return;
						}

						if (typeDefinition.HasGenericParameters)
						{
							Log.Error("SyncVar [" + fieldDefinition.FullName + "] cannot have generic parameters.");
							Weaver.fail = true;
							return;
						}

						if (typeDefinition.IsInterface)
						{
							Log.Error("SyncVar [" + fieldDefinition.FullName + "] cannot be an interface.");
							Weaver.fail = true;
							return;
						}

						var name = typeDefinition.Module.Name;
						if (name != Weaver.scriptDef.MainModule.Name && name != Weaver.UnityAssemblyDefinition.MainModule.Name && name != Weaver.QNetAssemblyDefinition.MainModule.Name && name != Weaver.corLib.Name && name != "System.Runtime.dll")
						{
							Log.Error($"SyncVar [{fieldDefinition.FullName}] is from an inaccessible module! : [{name}]");
							Weaver.fail = true;
							return;
						}

						if (fieldDefinition.FieldType.IsArray)
						{
							Log.Error("SyncVar [" + fieldDefinition.FullName + "] cannot be an array. Use a SyncList instead.");
							Weaver.fail = true;
							return;
						}

						if (Helpers.InheritsFromSyncList(fieldDefinition.FieldType))
						{
							Log.Warning(string.Format("Script class [{0}] has [SyncVar] attribute on SyncList field {1}, SyncLists should not be marked with SyncVar.", m_td.FullName, fieldDefinition.Name));
							break;
						}

						m_SyncVars.Add(fieldDefinition);
						ProcessSyncVar(fieldDefinition, 1 << num2);
						num2++;
						num++;
						if (num2 == 32)
						{
							Log.Error(string.Concat(new object[]
							{
								"Script class [",
								m_td.FullName,
								"] has too many SyncVars (",
								32,
								"). (This could include base classes)"
							}));
							Weaver.fail = true;
							return;
						}

						break;
					}
				}

				if (fieldDefinition.FieldType.FullName.Contains("UnityEngine.Networking.SyncListStruct"))
				{
					Log.Error("SyncListStruct member variable [" + fieldDefinition.FullName + "] must use a dervied class, like \"class MySyncList : SyncListStruct<MyStruct> {}\".");
					Weaver.fail = true;
					return;
				}

				if (Weaver.IsDerivedFrom(fieldDefinition.FieldType.Resolve(), Weaver.SyncListType))
				{
					if (fieldDefinition.IsStatic)
					{
						Log.Error(string.Concat(new string[]
						{
							"SyncList [",
							m_td.FullName,
							":",
							fieldDefinition.FullName,
							"] cannot be a static"
						}));
						Weaver.fail = true;
						return;
					}

					m_SyncVars.Add(fieldDefinition);
					m_SyncLists.Add(fieldDefinition);
					list.Add(ProcessSyncList(fieldDefinition, 1 << num2));
					num2++;
					num++;
					if (num2 == 32)
					{
						Log.Error(string.Concat(new object[]
						{
							"Script class [",
							m_td.FullName,
							"] has too many SyncVars (",
							32,
							"). (This could include base classes)"
						}));
						Weaver.fail = true;
						return;
					}
				}
			}

			foreach (var fieldDefinition2 in list)
			{
				m_td.Fields.Add(fieldDefinition2);
				m_SyncListStaticFields.Add(fieldDefinition2);
			}

			foreach (var item in m_SyncVarNetIds)
			{
				m_td.Fields.Add(item);
			}

			foreach (var item2 in m_SyncListInvocationFuncs)
			{
				m_td.Methods.Add(item2);
			}

			Weaver.SetNumSyncVars(m_td.FullName, num);
		}

		private static int GetHashCode(string s)
		{
			var assembly = typeof(Unity.UNetWeaver.Program).Assembly;
			var networkProcessorType = assembly.GetType("Unity.UNetWeaver.NetworkBehaviourProcessor");
			var methodInfo = networkProcessorType.GetMethod("GetHashCode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			return (int)methodInfo.Invoke(null, new object[] { s });
		}

		private bool HasMethod(string name)
		{
			foreach (var methodDefinition in m_td.Methods)
			{
				if (methodDefinition.Name == name)
				{
					return true;
				}
			}

			return false;
		}

		private readonly List<FieldDefinition> m_SyncVars = new();

		private readonly List<FieldDefinition> m_SyncLists = new();

		private readonly List<FieldDefinition> m_SyncVarNetIds = new();

		private readonly List<MethodDefinition> m_Cmds = new();

		private readonly List<MethodDefinition> m_Rpcs = new();

		private readonly List<MethodDefinition> m_TargetRpcs = new();

		private readonly List<EventDefinition> m_Events = new();

		private readonly List<FieldDefinition> m_SyncListStaticFields = new();

		private readonly List<MethodDefinition> m_CmdInvocationFuncs = new();

		private readonly List<MethodDefinition> m_SyncListInvocationFuncs = new();

		private readonly List<MethodDefinition> m_RpcInvocationFuncs = new();

		private readonly List<MethodDefinition> m_TargetRpcInvocationFuncs = new();

		private readonly List<MethodDefinition> m_EventInvocationFuncs = new();

		private readonly List<MethodDefinition> m_CmdCallFuncs = new();

		private readonly List<MethodDefinition> m_RpcCallFuncs = new();

		private readonly List<MethodDefinition> m_TargetRpcCallFuncs = new();

		private const int k_SyncVarLimit = 32;

		private int m_QosChannel;

		private readonly TypeDefinition m_td;

		private int m_NetIdFieldCounter;

		private const string k_CmdPrefix = "InvokeCmd";

		private const string k_RpcPrefix = "InvokeRpc";

		private const string k_TargetRpcPrefix = "InvokeTargetRpc";
	}
}
