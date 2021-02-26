using System;
using System.Linq;

namespace QSB.Utility
{
	public static class UnitTestDetector
	{
		public static bool IsInUnitTest { get; private set; }

		static UnitTestDetector()
		{
			var testAssemblyName = "Microsoft.VisualStudio.TestPlatform.TestFramework";
			IsInUnitTest = AppDomain.CurrentDomain.GetAssemblies()
				.Any(a => a.FullName.StartsWith(testAssemblyName));
		}
	}
}