using Mirror.Weaver;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace MirrorWeaver;

public static class ImprovedExtensions
{
	/// <summary>
	/// filters ONLY public fields instead of all non-private <br/>
	/// replaces generic parameter fields with their corresponding argument
	/// </summary>
	public static IEnumerable<FieldReference> FindAllPublicFields_Improved(this TypeReference tr)
	{
		Console.WriteLine($"FindAllPublicFields_Improved {tr}");
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

				FieldReference fr;
				if (tr is GenericInstanceType git &&
				    fd.FieldType is GenericParameter gp &&
				    gp.Owner == td)
				{
					fr = fd.SpecializeField(fd.Module, git);
					Console.WriteLine($"\t\t{fd.FieldType} -> {fr.FieldType}");
				}
				else
				{
					fr = fd.Module.ImportReference(fd);
				}

				Console.WriteLine($"\t\t{fd}");
				yield return fr;
			}

			tr = td.BaseType?.ApplyGenericParameters(tr);
		}
	}
}
