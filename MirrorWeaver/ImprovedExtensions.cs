using Mono.Cecil;
using System.Collections.Generic;

namespace MirrorWeaver
{
	public static class ImprovedExtensions
	{
		/// <summary>
		/// filters ONLY public fields instead of all non-private <br/>
		/// replaces generic parameter fields with their corresponding argument
		/// </summary>
		public static IEnumerable<FieldDefinition> FindAllPublicFields_Improved(this TypeReference tr)
		{
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

					if (fd.FieldType is GenericParameter gp && gp.Owner == td)
					{
						fd.FieldType = ((GenericInstanceType)tr).GenericArguments[gp.Position];
					}

					yield return fd;
				}

				tr = td.BaseType;
			}
		}
	}
}
