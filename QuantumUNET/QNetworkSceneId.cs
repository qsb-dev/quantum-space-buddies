using System;
using UnityEngine;

namespace QuantumUNET
{
	[Serializable]
	public struct QNetworkSceneId
	{
		[SerializeField]
		private uint m_Value;

		public QNetworkSceneId(uint value) => m_Value = value;

		public bool IsEmpty() => m_Value == 0U;

		public override int GetHashCode() => (int)m_Value;

		public override bool Equals(object obj) => obj is QNetworkSceneId id && this == id;

		public static bool operator ==(QNetworkSceneId c1, QNetworkSceneId c2) => c1.m_Value == c2.m_Value;

		public static bool operator !=(QNetworkSceneId c1, QNetworkSceneId c2) => c1.m_Value != c2.m_Value;

		public override string ToString() => m_Value.ToString();

		public uint Value => m_Value;
	}
}
