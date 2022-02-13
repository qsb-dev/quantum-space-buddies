using HarmonyLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
using MonoMod.Utils;
using QSB.Messaging;
using QSB.Utility;
using System;
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

			var fromField = typeof(QSBMessage).GetField("From", Util.AllInstance);
			var toField = typeof(QSBMessage).GetField("To", Util.AllInstance);
			var objectIdField = typeof(QSBWorldObjectMessage<>).GetField("ObjectId", Util.AllInstance);

			foreach (var type in messageTypes)
			{
				var fields = type.GetFields(Util.AllInstance)
					.Select(x => module.ImportReference(x).Resolve());

				var constructor = module.ImportReference(type.GetConstructors(Util.AllInstance).Single()).Resolve();
				var serialize = module.ImportReference(type.GetMethod("Serialize", Util.AllInstance)).Resolve();
				var deserialize = module.ImportReference(type.GetMethod("Deserialize", Util.AllInstance)).Resolve();

				foreach (var field in fields)
				{
					if (!field.Is(fromField) && !field.Is(toField) && !field.Is(objectIdField))
					{
						constructor.CheckUses(field, Util.UseType.Store);
					}

					serialize.CheckUses(field, Util.UseType.Load);
					deserialize.CheckUses(field, Util.UseType.Store);
				}
			}
		}
	}

	public static class Util
	{
		public static readonly BindingFlags AllInstance = AccessTools.all ^ BindingFlags.Static;

		public enum UseType { Store, Load }

		public static void CheckUses(this MethodDefinition method, FieldDefinition field, UseType useType)
		{
			Func<Instruction, bool> matches = useType switch
			{
				UseType.Store => x => x.MatchStfld(out var f) && f.Resolve() == field,
				UseType.Load => i => (i.MatchLdfld(out var f) || i.MatchLdflda(out f)) && f.Resolve() == field,
				_ => throw new ArgumentOutOfRangeException(nameof(useType), useType, null)
			};

			while (true)
			{
				var instructions = method.Body.Instructions;
				var uses = instructions.Any(matches);
				if (uses)
				{
					return;
				}

				var baseMethod = method.GetBaseMethod();
				if (baseMethod == method)
				{
					break;
				}

				var callsBase = instructions.Any(x => x.MatchCall(out var m) && m.Resolve() == baseMethod);
				if (!callsBase)
				{
					break;
				}

				method = baseMethod;
			}

			Assert.Fail($"{method} does not {useType} {field}");
		}
	}
}
