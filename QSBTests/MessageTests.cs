using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using QSB.Messaging;
using QSB.Utility;
using System.Linq;
using System.Reflection;

namespace QSBTests
{
	[TestClass]
	public class MessageTests
	{
		[TestMethod]
		public void TestMessages()
		{
			var module = ModuleDefinition.ReadModule("QSB.dll");
			var messageTypes = typeof(QSBMessage).GetDerivedTypes();

			var fromField = module.ImportReference(typeof(QSBMessage).GetField("From", Util.Flags));
			var toField = module.ImportReference(typeof(QSBMessage).GetField("To", Util.Flags));
			var objectIdField = module.ImportReference(typeof(QSBWorldObjectMessage<>).GetField("ObjectId", Util.Flags));

			foreach (var type in messageTypes)
			{
				var fields = type.GetFields(Util.Flags)
					.Select(x => module.ImportReference(x));

				var constructor = module.ImportReference(type.GetConstructors(Util.Flags).Single()).Resolve();
				var serialize = module.ImportReference(type.GetMethod("Serialize", Util.Flags)).Resolve();
				var deserialize = module.ImportReference(type.GetMethod("Deserialize", Util.Flags)).Resolve();

				foreach (var field in fields)
				{
					if (!field.Eq(fromField) && !field.Eq(toField) && !field.Eq(objectIdField))
					{
						constructor.CheckUses(field, Util.UseType.Store);
					}
					serialize.CheckUses(field, Util.UseType.Load);
					deserialize.CheckUses(field, Util.UseType.Store);
				}
			}
		}
	}

	internal static class Util
	{
		public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public static bool Eq(this MemberReference a, MemberReference b) =>
			a.DeclaringType.Namespace == b.DeclaringType.Namespace &&
			a.DeclaringType.Name == b.DeclaringType.Name &&
			a.Name == b.Name;

		public static bool IsOp<T>(this Instruction instruction, OpCode opCode, out T operand)
		{
			if (instruction.OpCode == opCode)
			{
				operand = (T)instruction.Operand;
				return true;
			}

			operand = default;
			return false;
		}

		public enum UseType { Store, Load };

		public static void CheckUses(this MethodDefinition method, FieldReference field, UseType useType)
		{
			var opCode = useType switch
			{
				UseType.Store => OpCodes.Stfld,
				UseType.Load => OpCodes.Ldfld,
			};

			while (true)
			{
				var il = method.Body.Instructions;
				var uses = il.Any(x =>
					x.IsOp(opCode, out FieldReference f) &&
					f.Eq(field)
				);
				if (uses)
				{
					return;
				}

				var baseMethod = method.GetBaseMethod();
				if (baseMethod.Eq(method))
				{
					break;
				}
				var callBase = il.Any(x =>
					x.IsOp(OpCodes.Call, out MethodReference m) &&
					m.Eq(baseMethod)
				);
				if (!callBase)
				{
					break;
				}
				method = baseMethod;
			}

			Assert.Fail($"{method} does not {useType} {field}");
		}
	}
}
