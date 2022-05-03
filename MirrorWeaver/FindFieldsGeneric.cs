using Mirror.Weaver;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace MirrorWeaver;

public static class FindFieldsGeneric
{
	/// <summary>
	/// filters ONLY public fields instead of all non-private <br/>
	/// replaces generic parameter fields with their corresponding argument
	/// </summary>
	private static IEnumerable<(TypeReference FieldType, FieldReference Fr)> FindAllPublicFields(this TypeReference tr)
	{
		var module = tr.Module;
		while (tr != null)
		{
			var td = tr.Resolve();
			foreach (var fd in td.Fields)
			{
				if (fd.IsStatic || !fd.IsPublic)
				{
					continue;
				}

				if (fd.IsNotSerialized)
				{
					continue;
				}

				var fieldType = fd.FieldType;
				var fr = module.ImportReference(fd);

				if (tr is GenericInstanceType git &&
				    fd.FieldType is GenericParameter gp &&
				    gp.Owner == td)
				{
					fieldType = git.GenericArguments[gp.Position];
					fr = fr.SpecializeField(module, git);
				}

				yield return (fieldType, fr);
			}

			tr = td.BaseType?.ApplyGenericParameters(tr);
		}
	}

	public static bool WriteAllFieldsGeneric(this Writers @this, TypeReference variable, ILProcessor worker, ref bool WeavingFailed)
	{
		foreach (var (fieldType, fr) in variable.FindAllPublicFields())
		{
			var writeFunc = @this.GetWriteFunc(fieldType, ref WeavingFailed);
			// need this null check till later PR when GetWriteFunc throws exception instead
			if (writeFunc == null) { return false; }

			worker.Emit(OpCodes.Ldarg_0);
			worker.Emit(OpCodes.Ldarg_1);
			worker.Emit(OpCodes.Ldfld, fr);
			worker.Emit(OpCodes.Call, writeFunc);
		}

		return true;
	}

	public static void ReadAllFieldsGeneric(this Readers @this, TypeReference tr, ILProcessor worker, ref bool WeavingFailed)
	{
		foreach (var (fieldType, fr) in tr.FindAllPublicFields())
		{
			// mismatched ldloca/ldloc for struct/class combinations is invalid IL, which causes crash at runtime
			var opcode = tr.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
			worker.Emit(opcode, 0);
			var readFunc = @this.GetReadFunc(fieldType, ref WeavingFailed);
			if (readFunc != null)
			{
				worker.Emit(OpCodes.Ldarg_0);
				worker.Emit(OpCodes.Call, readFunc);
			}
			else
			{
				@this.Log.Error($"{fr.Name} has an unsupported type", fr);
				WeavingFailed = true;
			}

			worker.Emit(OpCodes.Stfld, fr);
		}
	}
}
