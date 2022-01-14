using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
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

			var fromField = module.ImportReference(typeof(QSBMessage).GetField("From", Util.Flags));
			var toField = module.ImportReference(typeof(QSBMessage).GetField("To", Util.Flags));
			var objectIdField = module.ImportReference(typeof(QSBWorldObjectMessage<>).GetField("ObjectId", Util.Flags));

			foreach (var type in messageTypes)
			{
				var fields = type.GetFields(Util.Flags)
					.Select(x => module.ImportReference(x));

				var constructors = type.GetConstructors(Util.Flags);
				var constructor_ = constructors.Length > 1 ? constructors.Single(x => x.GetParameters().Length != 0) : constructors[0];
				var constructor = module.ImportReference(constructor_).Resolve();
				var serialize = module.ImportReference(type.GetMethod("Serialize", Util.Flags)).Resolve();
				var deserialize = module.ImportReference(type.GetMethod("Deserialize", Util.Flags)).Resolve();

				foreach (var field in fields)
				{
					if (!field.GenericEq(fromField) && !field.GenericEq(toField) && !field.GenericEq(objectIdField))
					{
						constructor.CheckUses(field, Util.UseType.Store);
					}

					// serialize.CheckUses(field, Util.UseType.Load);
					// deserialize.CheckUses(field, Util.UseType.Store);
				}
			}
		}
	}

	public static partial class Util
	{
		public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// ignores open vs closed generic type
		/// </summary>
		public static bool GenericEq(this MemberReference a, MemberReference b) =>
			a.DeclaringType.Namespace == b.DeclaringType.Namespace &&
			a.DeclaringType.Name == b.DeclaringType.Name &&
			a.Name == b.Name;

		public enum UseType { Store, Load }

		public static void CheckUses(this MethodDefinition method, FieldReference field, UseType useType)
		{
			Func<Instruction, bool> matches = useType switch
			{
				UseType.Store => x => x.MatchStfld(out var f) && f.GenericEq(field),
				UseType.Load => x => (x.MatchLdfld(out var f) || x.MatchLdflda(out f)) && f.GenericEq(field),
				_ => throw new ArgumentOutOfRangeException(nameof(useType), useType, null)
			};

			while (true)
			{
				var il = method.Body.Instructions;
				var uses = il.Any(matches);
				if (uses)
				{
					return;
				}

				var baseMethod = method.GetBaseMethod();
				if (baseMethod == method)
				{
					break;
				}

				var callsBase = il.Any(x => x.MatchCall(out var m) && m.GenericEq(baseMethod));
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
