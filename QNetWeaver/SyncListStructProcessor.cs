using Mono.Cecil;
using Mono.Cecil.Cil;

namespace QNetWeaver
{
	internal class SyncListStructProcessor
	{
		public SyncListStructProcessor(TypeDefinition typeDef)
		{
			Weaver.DLog(typeDef, "SyncListStructProcessor for " + typeDef.Name, new object[0]);
			m_TypeDef = typeDef;
		}

		public void Process()
		{
			var genericInstanceType = (GenericInstanceType)m_TypeDef.BaseType;
			if (genericInstanceType.GenericArguments.Count == 0)
			{
				Weaver.fail = true;
				Log.Error("SyncListStructProcessor no generic args");
			}
			else
			{
				m_ItemType = Weaver.scriptDef.MainModule.ImportReference(genericInstanceType.GenericArguments[0]);
				Weaver.DLog(m_TypeDef, "SyncListStructProcessor Start item:" + m_ItemType.FullName, new object[0]);
				Weaver.ResetRecursionCount();
				var methodReference = GenerateSerialization();
				if (!Weaver.fail)
				{
					var methodReference2 = GenerateDeserialization();
					if (methodReference2 != null && methodReference != null)
					{
						GenerateReadFunc(methodReference2);
						GenerateWriteFunc(methodReference);
						Weaver.DLog(m_TypeDef, "SyncListStructProcessor Done", new object[0]);
					}
				}
			}
		}

		private void GenerateReadFunc(MethodReference readItemFunc)
		{
			var text = "_ReadStruct" + m_TypeDef.Name + "_";
			if (m_TypeDef.DeclaringType != null)
			{
				text += m_TypeDef.DeclaringType.Name;
			}
			else
			{
				text += "None";
			}

			var methodDefinition = new MethodDefinition(text, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
			methodDefinition.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkReaderType)));
			methodDefinition.Parameters.Add(new ParameterDefinition("instance", ParameterAttributes.None, m_TypeDef));
			methodDefinition.Body.Variables.Add(new VariableDefinition(Weaver.uint16Type));
			methodDefinition.Body.Variables.Add(new VariableDefinition(Weaver.uint16Type));
			methodDefinition.Body.InitLocals = true;
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkReadUInt16));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
			var method = Helpers.MakeHostInstanceGeneric(Weaver.SyncListClear, new TypeReference[]
			{
				m_ItemType
			});
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, method));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_1));
			var instruction = ilprocessor.Create(OpCodes.Nop);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Br, instruction));
			var instruction2 = ilprocessor.Create(OpCodes.Nop);
			ilprocessor.Append(instruction2);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, readItemFunc));
			var self = Weaver.ResolveMethod(Weaver.SyncListStructType, "AddInternal");
			var method2 = Helpers.MakeHostInstanceGeneric(self, new TypeReference[]
			{
				m_ItemType
			});
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, method2));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Add));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Conv_U2));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_1));
			ilprocessor.Append(instruction);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Blt, instruction2));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			Weaver.RegisterReadByReferenceFunc(m_TypeDef.FullName, methodDefinition);
		}

		private void GenerateWriteFunc(MethodReference writeItemFunc)
		{
			var text = "_WriteStruct" + m_TypeDef.GetElementType().Name + "_";
			if (m_TypeDef.DeclaringType != null)
			{
				text += m_TypeDef.DeclaringType.Name;
			}
			else
			{
				text += "None";
			}

			var methodDefinition = new MethodDefinition(text, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig, Weaver.voidType);
			methodDefinition.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkWriterType)));
			methodDefinition.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(m_TypeDef)));
			methodDefinition.Body.Variables.Add(new VariableDefinition(Weaver.uint16Type));
			methodDefinition.Body.Variables.Add(new VariableDefinition(Weaver.uint16Type));
			methodDefinition.Body.InitLocals = true;
			var ilprocessor = methodDefinition.Body.GetILProcessor();
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
			var self = Weaver.ResolveMethod(Weaver.SyncListStructType, "get_Count");
			var method = Helpers.MakeHostInstanceGeneric(self, new TypeReference[]
			{
				m_ItemType
			});
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, method));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, Weaver.NetworkWriteUInt16));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_1));
			var instruction = ilprocessor.Create(OpCodes.Nop);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Br, instruction));
			var instruction2 = ilprocessor.Create(OpCodes.Nop);
			ilprocessor.Append(instruction2);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
			var self2 = Weaver.ResolveMethod(Weaver.SyncListStructType, "GetItem");
			var method2 = Helpers.MakeHostInstanceGeneric(self2, new TypeReference[]
			{
				m_ItemType
			});
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, method2));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Callvirt, writeItemFunc));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldc_I4_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Add));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Conv_U2));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Stloc_1));
			ilprocessor.Append(instruction);
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_1));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Blt, instruction2));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			Weaver.RegisterWriteFunc(m_TypeDef.FullName, methodDefinition);
		}

		private MethodReference GenerateSerialization()
		{
			Weaver.DLog(m_TypeDef, "  SyncListStruct GenerateSerialization", new object[0]);
			foreach (var methodDefinition in m_TypeDef.Methods)
			{
				if (methodDefinition.Name == "SerializeItem")
				{
					Weaver.DLog(m_TypeDef, "  Abort - is SerializeItem", new object[0]);
					return methodDefinition;
				}
			}

			var methodDefinition2 = new MethodDefinition("SerializeItem", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, Weaver.voidType);
			methodDefinition2.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkWriterType)));
			methodDefinition2.Parameters.Add(new ParameterDefinition("item", ParameterAttributes.None, m_ItemType));
			var ilprocessor = methodDefinition2.Body.GetILProcessor();
			MethodReference result;
			if (m_ItemType.IsGenericInstance)
			{
				Weaver.fail = true;
				Log.Error("GenerateSerialization for " + Helpers.PrettyPrintType(m_ItemType) + " failed. Struct passed into SyncListStruct<T> can't have generic parameters");
				result = null;
			}
			else
			{
				foreach (var fieldDefinition in m_ItemType.Resolve().Fields)
				{
					if (!fieldDefinition.IsStatic && !fieldDefinition.IsPrivate && !fieldDefinition.IsSpecialName)
					{
						var fieldReference = Weaver.scriptDef.MainModule.ImportReference(fieldDefinition);
						var typeDefinition = fieldReference.FieldType.Resolve();
						if (typeDefinition.HasGenericParameters)
						{
							Weaver.fail = true;
							Log.Error(string.Concat(new object[]
							{
								"GenerateSerialization for ",
								m_TypeDef.Name,
								" [",
								typeDefinition,
								"/",
								typeDefinition.FullName,
								"]. UNet [MessageBase] member cannot have generic parameters."
							}));
							return null;
						}

						if (typeDefinition.IsInterface)
						{
							Weaver.fail = true;
							Log.Error(string.Concat(new object[]
							{
								"GenerateSerialization for ",
								m_TypeDef.Name,
								" [",
								typeDefinition,
								"/",
								typeDefinition.FullName,
								"]. UNet [MessageBase] member cannot be an interface."
							}));
							return null;
						}

						var writeFunc = Weaver.GetWriteFunc(fieldDefinition.FieldType);
						if (writeFunc == null)
						{
							Weaver.fail = true;
							Log.Error(string.Concat(new object[]
							{
								"GenerateSerialization for ",
								m_TypeDef.Name,
								" unknown type [",
								typeDefinition,
								"/",
								typeDefinition.FullName,
								"]. UNet [MessageBase] member variables must be basic types."
							}));
							return null;
						}

						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_2));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldReference));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Call, writeFunc));
					}
				}

				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				m_TypeDef.Methods.Add(methodDefinition2);
				result = methodDefinition2;
			}

			return result;
		}

		private MethodReference GenerateDeserialization()
		{
			Weaver.DLog(m_TypeDef, "  GenerateDeserialization", new object[0]);
			foreach (var methodDefinition in m_TypeDef.Methods)
			{
				if (methodDefinition.Name == "DeserializeItem")
				{
					return methodDefinition;
				}
			}

			var methodDefinition2 = new MethodDefinition("DeserializeItem", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, m_ItemType);
			methodDefinition2.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkReaderType)));
			var ilprocessor = methodDefinition2.Body.GetILProcessor();
			ilprocessor.Body.InitLocals = true;
			ilprocessor.Body.Variables.Add(new VariableDefinition(m_ItemType));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloca, 0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Initobj, m_ItemType));
			foreach (var fieldDefinition in m_ItemType.Resolve().Fields)
			{
				if (!fieldDefinition.IsStatic && !fieldDefinition.IsPrivate && !fieldDefinition.IsSpecialName)
				{
					var fieldReference = Weaver.scriptDef.MainModule.ImportReference(fieldDefinition);
					var typeDefinition = fieldReference.FieldType.Resolve();
					var readFunc = Weaver.GetReadFunc(fieldDefinition.FieldType);
					if (readFunc == null)
					{
						Weaver.fail = true;
						Log.Error(string.Concat(new object[]
						{
							"GenerateDeserialization for ",
							m_TypeDef.Name,
							" unknown type [",
							typeDefinition,
							"]. UNet [SyncVar] member variables must be basic types."
						}));
						return null;
					}

					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloca, 0));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Call, readFunc));
					ilprocessor.Append(ilprocessor.Create(OpCodes.Stfld, fieldReference));
				}
			}

			ilprocessor.Append(ilprocessor.Create(OpCodes.Ldloc_0));
			ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
			m_TypeDef.Methods.Add(methodDefinition2);
			return methodDefinition2;
		}

		private readonly TypeDefinition m_TypeDef;

		private TypeReference m_ItemType;
	}
}
