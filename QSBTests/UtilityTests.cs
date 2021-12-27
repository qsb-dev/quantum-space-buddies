using Microsoft.VisualStudio.TestTools.UnitTesting;
using OWML.Utils;
using QSB.Utility;
using System;

namespace QSBTests
{
	[TestClass]
	public class UtilityTests
	{
		[TestMethod]
		public void IsInUnitTest()
			=> Assert.IsTrue(UnitTestDetector.IsInUnitTest, "UnitTestDetector is not working.");


		private class A
		{
			public virtual string Method() => nameof(A);
			public virtual string MethodArgs(int num) => nameof(A);
			public virtual void VoidMethod() { }
			public virtual void GenericMethod<T>(T t) { }
		}

		private class B : A
		{
			public override string Method() => nameof(B);
			public override string MethodArgs(int num) => nameof(B);
			public override void VoidMethod() { }
			public override void GenericMethod<T>(T t) { }
		}

		[TestMethod]
		public void TestInvokeBase()
		{
			var a = new A();
			Assert.AreEqual(nameof(A), a.Method());
			A b = new B();
			Assert.AreEqual(nameof(B), b.Method());

			Assert.AreEqual(nameof(B), b.Invoke<string>(nameof(b.Method)));

			Assert.AreEqual(nameof(A), b.InvokeBase<string>(nameof(b.Method)));
			Assert.AreEqual(nameof(A), b.InvokeBase<string>(nameof(b.MethodArgs), 1));
			b.InvokeBase(nameof(b.VoidMethod));
			Assert.ThrowsException<MissingMethodException>(() =>
				b.InvokeBase(nameof(b.GenericMethod), (object)null));
		}
	}
}
