using Microsoft.VisualStudio.TestTools.UnitTesting;
using QSB.Utility;

namespace QSBTests
{
	[TestClass]
	public class UtilityTests
	{
		[TestMethod]
		public void IsInUnitTest() 
			=> Assert.IsTrue(UnitTestDetector.IsInUnitTest, "UnitTestDetector is not working.");
	}
}
