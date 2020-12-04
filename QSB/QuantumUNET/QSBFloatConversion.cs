using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.QuantumUNET
{
	internal class QSBFloatConversion
	{
		public static float ToSingle(uint value)
		{
			QSBUIntFloat uintFloat = default(QSBUIntFloat);
			uintFloat.intValue = value;
			return uintFloat.floatValue;
		}

		public static double ToDouble(ulong value)
		{
			QSBUIntFloat uintFloat = default(QSBUIntFloat);
			uintFloat.longValue = value;
			return uintFloat.doubleValue;
		}

		public static decimal ToDecimal(ulong value1, ulong value2)
		{
			QSBUIntDecimal uintDecimal = default(QSBUIntDecimal);
			uintDecimal.longValue1 = value1;
			uintDecimal.longValue2 = value2;
			return uintDecimal.decimalValue;
		}
	}
}
