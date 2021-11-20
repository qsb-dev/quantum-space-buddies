using Mono.Cecil;
using System.Collections.Generic;

namespace QNetWeaver
{
	internal class WeaverLists
	{
		public List<FieldDefinition> replacedFields = new();

		public List<MethodDefinition> replacementProperties = new();

		public List<FieldDefinition> netIdFields = new();

		public List<MethodDefinition> replacedMethods = new();

		public List<MethodDefinition> replacementMethods = new();

		public HashSet<string> replacementMethodNames = new();

		public List<EventDefinition> replacedEvents = new();

		public List<MethodDefinition> replacementEvents = new();

		public Dictionary<string, MethodReference> readFuncs;

		public Dictionary<string, MethodReference> readByReferenceFuncs;

		public Dictionary<string, MethodReference> writeFuncs;

		public List<MethodDefinition> generatedReadFunctions = new();

		public List<MethodDefinition> generatedWriteFunctions = new();

		public TypeDefinition generateContainerClass;

		public Dictionary<string, int> numSyncVars = new();
	}
}
