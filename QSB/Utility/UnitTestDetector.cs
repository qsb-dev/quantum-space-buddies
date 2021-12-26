using System;
using System.Linq;

namespace QSB.Utility
{
	public static class UnitTestDetector
	{
		public static readonly bool IsInUnitTest;

		static UnitTestDetector()
		{
			const string testAssemblyName = "Microsoft.VisualStudio.TestPlatform.TestFramework";
			IsInUnitTest = AppDomain.CurrentDomain.GetAssemblies()
				.Any(a => a.FullName.StartsWith(testAssemblyName));
		}
	}
}