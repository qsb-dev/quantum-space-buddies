using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mirror.Weaver
{
	public static class Extensions
	{
		public static bool Is(this TypeReference td, Type t)
		{
			if (t.IsGenericType)
			{
				return td.GetElementType().FullName == t.FullName;
			}
			return td.FullName == t.FullName;
		}

		public static bool Is<T>(this TypeReference td) => Is(td, typeof(T));

		public static bool IsDerivedFrom<T>(this TypeReference tr) => IsDerivedFrom(tr, typeof(T));

		public static bool IsDerivedFrom(this TypeReference tr, Type baseClass)
		{
			if (tr == null)
			{
				return false;
			}

			var td = tr.Resolve();
			if (td == null)
			{
				return false;
			}

			if (!td.IsClass)
			{
				return false;
			}

			// are ANY parent classes of baseClass?
			var parent = td.BaseType;

			if (parent == null)
			{
				return false;
			}

			if (parent.Is(baseClass))
			{
				return true;
			}

			if (parent.CanBeResolved())
			{
				return IsDerivedFrom(parent.Resolve(), baseClass);
			}

			return false;
		}

		public static TypeReference GetEnumUnderlyingType(this TypeDefinition td)
		{
			foreach (var field in td.Fields)
			{
				if (!field.IsStatic)
				{
					return field.FieldType;
				}
			}
			throw new ArgumentException($"Invalid enum {td.FullName}");
		}

		public static bool ImplementsInterface<TInterface>(this TypeDefinition td)
		{
			var typedef = td;

			while (typedef != null)
			{
				if (typedef.Interfaces.Any(iface => iface.InterfaceType.Is<TInterface>()))
				{
					return true;
				}

				try
				{
					var parent = typedef.BaseType;
					typedef = parent?.Resolve();
				}
				catch (AssemblyResolutionException)
				{
					// this can happen for plugins.
					//Console.WriteLine("AssemblyResolutionException: "+ ex.ToString());
					break;
				}
			}

			return false;
		}

		public static bool IsMultidimensionalArray(this TypeReference tr) =>
			tr is ArrayType arrayType && arrayType.Rank > 1;

		// Does type use netId as backing field
		public static bool IsNetworkIdentityField(this TypeReference tr) =>
			tr.Is<UnityEngine.GameObject>() ||
			tr.Is<NetworkIdentity>() ||
			tr.IsDerivedFrom<NetworkBehaviour>();

		public static bool CanBeResolved(this TypeReference parent)
		{
			while (parent != null)
			{
				if (parent.Scope.Name == "Windows")
				{
					return false;
				}

				if (parent.Scope.Name == "mscorlib")
				{
					var resolved = parent.Resolve();
					return resolved != null;
				}

				try
				{
					parent = parent.Resolve().BaseType;
				}
				catch
				{
					return false;
				}
			}
			return true;
		}

		// Makes T => Variable and imports function
		public static MethodReference MakeGeneric(this MethodReference generic, ModuleDefinition module, TypeReference variableReference)
		{
			var instance = new GenericInstanceMethod(generic);
			instance.GenericArguments.Add(variableReference);

			var readFunc = module.ImportReference(instance);
			return readFunc;
		}

		// Given a method of a generic class such as ArraySegment`T.get_Count,
		// and a generic instance such as ArraySegment`int
		// Creates a reference to the specialized method  ArraySegment`int`.get_Count
		// Note that calling ArraySegment`T.get_Count directly gives an invalid IL error
		public static MethodReference MakeHostInstanceGeneric(this MethodReference self, ModuleDefinition module, GenericInstanceType instanceType)
		{
			var reference = new MethodReference(self.Name, self.ReturnType, instanceType)
			{
				CallingConvention = self.CallingConvention,
				HasThis = self.HasThis,
				ExplicitThis = self.ExplicitThis
			};

			foreach (var parameter in self.Parameters)
			{
				reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
			}

			foreach (var generic_parameter in self.GenericParameters)
			{
				reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));
			}

			return module.ImportReference(reference);
		}

		// Given a field of a generic class such as Writer<T>.write,
		// and a generic instance such as ArraySegment`int
		// Creates a reference to the specialized method  ArraySegment`int`.get_Count
		// Note that calling ArraySegment`T.get_Count directly gives an invalid IL error
		public static FieldReference SpecializeField(this FieldReference self, ModuleDefinition module, GenericInstanceType instanceType)
		{
			var reference = new FieldReference(self.Name, self.FieldType, instanceType);
			return module.ImportReference(reference);
		}

		public static CustomAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeProvider method) => method.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Is<TAttribute>());

		public static bool HasCustomAttribute<TAttribute>(this ICustomAttributeProvider attributeProvider) => attributeProvider.CustomAttributes.Any(attr => attr.AttributeType.Is<TAttribute>());

		public static T GetField<T>(this CustomAttribute ca, string field, T defaultValue)
		{
			foreach (var customField in ca.Fields)
			{
				if (customField.Name == field)
				{
					return (T)customField.Argument.Value;
				}
			}

			return defaultValue;
		}

		public static MethodDefinition GetMethod(this TypeDefinition td, string methodName) => td.Methods.FirstOrDefault(method => method.Name == methodName);

		public static List<MethodDefinition> GetMethods(this TypeDefinition td, string methodName) => td.Methods.Where(method => method.Name == methodName).ToList();

		public static MethodDefinition GetMethodInBaseType(this TypeDefinition td, string methodName)
		{
			var typedef = td;
			while (typedef != null)
			{
				foreach (var md in typedef.Methods)
				{
					if (md.Name == methodName)
					{
						return md;
					}
				}

				try
				{
					var parent = typedef.BaseType;
					typedef = parent?.Resolve();
				}
				catch (AssemblyResolutionException)
				{
					// this can happen for plugins.
					break;
				}
			}

			return null;
		}

		// Finds public fields in type and base type
		public static IEnumerable<FieldDefinition> FindAllPublicFields(this TypeReference variable) => FindAllPublicFields(variable.Resolve());

		// Finds public fields in type and base type
		public static IEnumerable<FieldDefinition> FindAllPublicFields(this TypeDefinition typeDefinition)
		{
			while (typeDefinition != null)
			{
				foreach (var field in typeDefinition.Fields)
				{
					if (field.IsStatic || field.IsPrivate)
					{
						continue;
					}

					if (field.IsNotSerialized)
					{
						continue;
					}

					yield return field;
				}

				try
				{
					typeDefinition = typeDefinition.BaseType?.Resolve();
				}
				catch (AssemblyResolutionException)
				{
					break;
				}
			}
		}

		public static bool ContainsClass(this ModuleDefinition module, string nameSpace, string className) =>
			module.GetTypes().Any(td => td.Namespace == nameSpace &&
								  td.Name == className);


		public static AssemblyNameReference FindReference(this ModuleDefinition module, string referenceName)
		{
			foreach (var reference in module.AssemblyReferences)
			{
				if (reference.Name == referenceName)
				{
					return reference;
				}
			}
			return null;
		}
	}
}
