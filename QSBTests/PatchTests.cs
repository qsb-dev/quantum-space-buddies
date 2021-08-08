using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSB.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QSBTests
{
	[TestClass]
	public class PatchTests
	{
		[TestMethod]
		public void CheckUnreferencedPatches()
		{
			var qsbAssembly = Assembly.Load("QSB");
			var allPatchTypes = qsbAssembly
				.GetTypes()
				.Where(x => typeof(QSBPatch).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

			QSBPatchManager.Init();
			var patchInstances = (List<QSBPatch>)typeof(QSBPatchManager)
				.GetField("_patchList", BindingFlags.NonPublic | BindingFlags.Static)
				.GetValue(typeof(QSBPatchManager));

			var failedTypes = new List<Type>();
			foreach (var type in allPatchTypes)
			{
				if (!patchInstances.Any(x => x.GetType() == type))
				{
					failedTypes.Add(type);
				}
			}

			if (failedTypes.Count > 0)
			{
				Assert.Fail(string.Join(", ", failedTypes.Select(x => x.Name)));
			}
		}
	}
}
