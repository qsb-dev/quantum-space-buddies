using Mono.Cecil;
using System.Collections.Generic;

namespace QNetWeaver
{
	internal class WeaverLists
	{
		public List<FieldDefinition> replacedFields = new List<FieldDefinition>();

		public List<MethodDefinition> replacementProperties = new List<MethodDefinition>();

		public List<FieldDefinition> netIdFields = new List<FieldDefinition>();

		public List<MethodDefinition> replacedMethods = new List<MethodDefinition>();

		public List<MethodDefinition> replacementMethods = new List<MethodDefinition>();

		public HashSet<string> replacementMethodNames = new HashSet<string>();

		public List<EventDefinition> replacedEvents = new List<EventDefinition>();

		public List<MethodDefinition> replacementEvents = new List<MethodDefinition>();

		public Dictionary<string, MethodReference> readFuncs;

		public Dictionary<string, MethodReference> readByReferenceFuncs;

		public Dictionary<string, MethodReference> writeFuncs;

		public List<MethodDefinition> generatedReadFunctions = new List<MethodDefinition>();

		public List<MethodDefinition> generatedWriteFunctions = new List<MethodDefinition>();

		public TypeDefinition generateContainerClass;

		public Dictionary<string, int> numSyncVars = new Dictionary<string, int>();
	}
}
