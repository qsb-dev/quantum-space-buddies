using System;
using System.Linq;

namespace QSB.Utility
{
    public static class UnitTestDetector
    {
        static UnitTestDetector()
        {
            var testAssemblyName = "Microsoft.VisualStudio.TestPlatform.TestFramework";
            IsInUnitTest = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.FullName.StartsWith(testAssemblyName));
        }

        public static bool IsInUnitTest { get; private set; }
    }
}