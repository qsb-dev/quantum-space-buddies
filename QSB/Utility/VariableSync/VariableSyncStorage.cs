using System.Collections.Generic;
using System.Linq;

namespace QSB.Utility.VariableSync;

public static class VariableSyncStorage
{
	public static List<IWorldObjectVariableSyncer> _instances = new();

	public static List<U> GetSpecificSyncers<U>()
		=> _instances.OfType<U>().ToList();
}
