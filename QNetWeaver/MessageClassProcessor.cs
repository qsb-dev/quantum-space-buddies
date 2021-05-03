using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QNetWeaver
{
	internal class MessageClassProcessor
	{
		public MessageClassProcessor(TypeDefinition td)
		{
			Weaver.DLog(td, "MessageClassProcessor for " + td.Name, new object[0]);
			this.m_td = td;
		}

		public void Process()
		{
			Weaver.DLog(this.m_td, "MessageClassProcessor Start", new object[0]);
			Weaver.ResetRecursionCount();
			this.GenerateSerialization();
			if (!Weaver.fail)
			{
				this.GenerateDeSerialization();
				Weaver.DLog(this.m_td, "MessageClassProcessor Done", new object[0]);
			}
		}

		private void GenerateSerialization()
		{
			Weaver.DLog(this.m_td, "  MessageClass GenerateSerialization", new object[0]);
			foreach (var methodDefinition in this.m_td.Methods)
			{
				if (methodDefinition.Name == "Serialize")
				{
					Weaver.DLog(this.m_td, "  Abort - is Serialize", new object[0]);
					return;
				}
			}
			if (this.m_td.Fields.Count != 0)
			{
				foreach (var fieldDefinition in this.m_td.Fields)
				{
					if (fieldDefinition.FieldType.FullName == this.m_td.FullName)
					{
						Weaver.fail = true;
						Log.Error(string.Concat(new string[]
						{
							"GenerateSerialization for ",
							this.m_td.Name,
							" [",
							fieldDefinition.FullName,
							"]. [MessageBase] member cannot be self referencing."
						}));
						return;
					}
				}
				var methodDefinition2 = new MethodDefinition("Serialize", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, Weaver.voidType);
				methodDefinition2.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkWriterType)));
				var ilprocessor = methodDefinition2.Body.GetILProcessor();
				foreach (var fieldDefinition2 in this.m_td.Fields)
				{
					if (!fieldDefinition2.IsStatic && !fieldDefinition2.IsPrivate && !fieldDefinition2.IsSpecialName)
					{
						if (fieldDefinition2.FieldType.Resolve().HasGenericParameters)
						{
							Weaver.fail = true;
							Log.Error(string.Concat(new object[]
							{
								"GenerateSerialization for ",
								this.m_td.Name,
								" [",
								fieldDefinition2.FieldType,
								"/",
								fieldDefinition2.FieldType.FullName,
								"]. [MessageBase] member cannot have generic parameters."
							}));
							return;
						}
						if (fieldDefinition2.FieldType.Resolve().IsInterface)
						{
							Weaver.fail = true;
							Log.Error(string.Concat(new object[]
							{
								"GenerateSerialization for ",
								this.m_td.Name,
								" [",
								fieldDefinition2.FieldType,
								"/",
								fieldDefinition2.FieldType.FullName,
								"]. [MessageBase] member cannot be an interface."
							}));
							return;
						}
						var writeFunc = Weaver.GetWriteFunc(fieldDefinition2.FieldType);
						if (writeFunc == null)
						{
							Weaver.fail = true;
							Log.Error(string.Concat(new object[]
							{
								"GenerateSerialization for ",
								this.m_td.Name,
								" unknown type [",
								fieldDefinition2.FieldType,
								"/",
								fieldDefinition2.FieldType.FullName,
								"]. [MessageBase] member variables must be basic types."
							}));
							return;
						}
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldfld, fieldDefinition2));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Call, writeFunc));
					}
				}
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				this.m_td.Methods.Add(methodDefinition2);
			}
		}

		private void GenerateDeSerialization()
		{
			Weaver.DLog(this.m_td, "  GenerateDeserialization", new object[0]);
			foreach (var methodDefinition in this.m_td.Methods)
			{
				if (methodDefinition.Name == "Deserialize")
				{
					return;
				}
			}
			if (this.m_td.Fields.Count != 0)
			{
				var methodDefinition2 = new MethodDefinition("Deserialize", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig, Weaver.voidType);
				methodDefinition2.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, Weaver.scriptDef.MainModule.ImportReference(Weaver.NetworkReaderType)));
				var ilprocessor = methodDefinition2.Body.GetILProcessor();
				foreach (var fieldDefinition in this.m_td.Fields)
				{
					if (!fieldDefinition.IsStatic && !fieldDefinition.IsPrivate && !fieldDefinition.IsSpecialName)
					{
						var readFunc = Weaver.GetReadFunc(fieldDefinition.FieldType);
						if (readFunc == null)
						{
							Weaver.fail = true;
							Log.Error(string.Concat(new object[]
							{
								"GenerateDeSerialization for ",
								this.m_td.Name,
								" unknown type [",
								fieldDefinition.FieldType,
								"]. [SyncVar] member variables must be basic types."
							}));
							return;
						}
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_0));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Ldarg_1));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Call, readFunc));
						ilprocessor.Append(ilprocessor.Create(OpCodes.Stfld, fieldDefinition));
					}
				}
				ilprocessor.Append(ilprocessor.Create(OpCodes.Ret));
				this.m_td.Methods.Add(methodDefinition2);
			}
		}

		private TypeDefinition m_td;
	}
}
