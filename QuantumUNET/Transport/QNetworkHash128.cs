using System;

namespace QuantumUNET.Transport
{
	[Serializable]
	public struct QNetworkHash128
	{
		public byte i0;
		public byte i1;
		public byte i2;
		public byte i3;
		public byte i4;
		public byte i5;
		public byte i6;
		public byte i7;
		public byte i8;
		public byte i9;
		public byte i10;
		public byte i11;
		public byte i12;
		public byte i13;
		public byte i14;
		public byte i15;

		public void Reset()
		{
			i0 = 0;
			i1 = 0;
			i2 = 0;
			i3 = 0;
			i4 = 0;
			i5 = 0;
			i6 = 0;
			i7 = 0;
			i8 = 0;
			i9 = 0;
			i10 = 0;
			i11 = 0;
			i12 = 0;
			i13 = 0;
			i14 = 0;
			i15 = 0;
		}

		public bool IsValid()
		{
			return (i0 | i1 | i2 | i3 | i4 | i5 | i6 | i7 | i8 | i9 | i10 | i11 | i12 | i13 | i14 | i15) != 0;
		}

		private static int HexToNumber(char c)
		{
			int result;
			if (c is >= '0' and <= '9')
			{
				result = c - '0';
			}
			else if (c is >= 'a' and <= 'f')
			{
				result = c - 'a' + '\n';
			}
			else if (c is >= 'A' and <= 'F')
			{
				result = c - 'A' + '\n';
			}
			else
			{
				result = 0;
			}

			return result;
		}

		public static QNetworkHash128 Parse(string text)
		{
			var length = text.Length;
			if (length < 32)
			{
				var str = "";
				for (var i = 0; i < 32 - length; i++)
				{
					str += "0";
				}

				text = str + text;
			}

			QNetworkHash128 result;
			result.i0 = (byte)(HexToNumber(text[0]) * 16 + HexToNumber(text[1]));
			result.i1 = (byte)(HexToNumber(text[2]) * 16 + HexToNumber(text[3]));
			result.i2 = (byte)(HexToNumber(text[4]) * 16 + HexToNumber(text[5]));
			result.i3 = (byte)(HexToNumber(text[6]) * 16 + HexToNumber(text[7]));
			result.i4 = (byte)(HexToNumber(text[8]) * 16 + HexToNumber(text[9]));
			result.i5 = (byte)(HexToNumber(text[10]) * 16 + HexToNumber(text[11]));
			result.i6 = (byte)(HexToNumber(text[12]) * 16 + HexToNumber(text[13]));
			result.i7 = (byte)(HexToNumber(text[14]) * 16 + HexToNumber(text[15]));
			result.i8 = (byte)(HexToNumber(text[16]) * 16 + HexToNumber(text[17]));
			result.i9 = (byte)(HexToNumber(text[18]) * 16 + HexToNumber(text[19]));
			result.i10 = (byte)(HexToNumber(text[20]) * 16 + HexToNumber(text[21]));
			result.i11 = (byte)(HexToNumber(text[22]) * 16 + HexToNumber(text[23]));
			result.i12 = (byte)(HexToNumber(text[24]) * 16 + HexToNumber(text[25]));
			result.i13 = (byte)(HexToNumber(text[26]) * 16 + HexToNumber(text[27]));
			result.i14 = (byte)(HexToNumber(text[28]) * 16 + HexToNumber(text[29]));
			result.i15 = (byte)(HexToNumber(text[30]) * 16 + HexToNumber(text[31]));
			return result;
		}

		public override string ToString() =>
			$"{i0:x2}{i1:x2}{i2:x2}{i3:x2}{i4:x2}{i5:x2}{i6:x2}{i7:x2}"
			+ $"{i8:x2}{i9:x2}{i10:x2}{i11:x2}{i12:x2}{i13:x2}{i14:x2}{i15:x2}";
	}
}
