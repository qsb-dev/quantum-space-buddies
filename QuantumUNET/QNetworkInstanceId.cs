using System;
using UnityEngine;

namespace QuantumUNET
{
	[Serializable]
	public struct QNetworkInstanceId
	{
		[SerializeField]
		private readonly uint m_Value;

		public static QNetworkInstanceId Invalid = new QNetworkInstanceId(uint.MaxValue);

		internal static QNetworkInstanceId Zero = new QNetworkInstanceId(0U);

		public QNetworkInstanceId(uint value) => m_Value = value;

		public bool IsEmpty() => m_Value == 0U;

		public override int GetHashCode() => (int)m_Value;

		public override bool Equals(object obj) => obj is QNetworkInstanceId id && this == id;

		public static bool operator ==(QNetworkInstanceId c1, QNetworkInstanceId c2) => c1.m_Value == c2.m_Value;

		public static bool operator !=(QNetworkInstanceId c1, QNetworkInstanceId c2) => c1.m_Value != c2.m_Value;

		public override string ToString() => m_Value.ToString();

		public uint Value => m_Value;
	}
}
