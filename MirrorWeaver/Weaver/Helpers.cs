using Mono.Cecil;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mirror.Weaver
{
	internal static class Helpers
	{
		// This code is taken from SerializationWeaver
		public static string UnityEngineDllDirectoryName()
		{
			var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
			return directoryName?.Replace(@"file:\", "");
		}

		public static bool IsEditorAssembly(AssemblyDefinition currentAssembly) =>
			// we want to add the [InitializeOnLoad] attribute if it's available
			// -> usually either 'UnityEditor' or 'UnityEditor.CoreModule'
			currentAssembly.MainModule.AssemblyReferences.Any(assemblyReference =>
				assemblyReference.Name.StartsWith(nameof(UnityEditor))
			);
	}
}
