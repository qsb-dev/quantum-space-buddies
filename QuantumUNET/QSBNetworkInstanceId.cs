using System;
using UnityEngine;

namespace QuantumUNET
{
	[Serializable]
	public struct QSBNetworkInstanceId
	{
		public uint Value => m_Value;

		[SerializeField]
		private readonly uint m_Value;

		public QSBNetworkInstanceId(uint value)
		{
			m_Value = value;
		}

		public bool IsEmpty()
			=> m_Value == 0U;

		public override int GetHashCode()
			=> (int)m_Value;

		public override bool Equals(object obj)
			=> obj is QSBNetworkInstanceId id && this == id;

		public static bool operator ==(QSBNetworkInstanceId c1, QSBNetworkInstanceId c2)
			=> c1.m_Value == c2.m_Value;

		public static bool operator !=(QSBNetworkInstanceId c1, QSBNetworkInstanceId c2)
			=> c1.m_Value != c2.m_Value;

		public override string ToString()
			=> m_Value.ToString();

		public static QSBNetworkInstanceId Invalid = new QSBNetworkInstanceId(uint.MaxValue);

		internal static QSBNetworkInstanceId Zero = new QSBNetworkInstanceId(0U);
	}
}
