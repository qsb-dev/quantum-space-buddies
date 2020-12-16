using System;
using UnityEngine;
using UnityEngine.Networking;

namespace QuantumUNET
{
	[Serializable]
	public struct QSBNetworkSceneId
	{
		public uint Value => m_Value;

		[SerializeField]
		private uint m_Value;

		public QSBNetworkSceneId(uint value)
		{
			m_Value = value;
		}

		public bool IsEmpty()
			=> m_Value == 0U;

		public override int GetHashCode()
			=> (int)m_Value;

		public override bool Equals(object obj)
			=> obj is QSBNetworkSceneId id && this == id;

		public static bool operator ==(QSBNetworkSceneId c1, QSBNetworkSceneId c2)
			=> c1.m_Value == c2.m_Value;

		public static bool operator !=(QSBNetworkSceneId c1, QSBNetworkSceneId c2)
			=> c1.m_Value != c2.m_Value;

		public override string ToString() => m_Value.ToString();

		public static explicit operator QSBNetworkSceneId(NetworkSceneId v)
		{
			return new QSBNetworkSceneId
			{
				m_Value = v.Value
			};
		}
	}
}
