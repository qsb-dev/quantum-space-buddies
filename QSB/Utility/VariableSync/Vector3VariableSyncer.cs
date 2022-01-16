using Mirror;
using System;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public class Vector3VariableSyncer : BaseVariableSyncer
	{
		private Vector3 _prevValue;
		[NonSerialized]
		public Vector3 Value;

		protected override bool HasChanged() => Value != _prevValue;
		protected override void UpdatePrevData() => _prevValue = Value;
		protected override void Serialize(NetworkWriter writer, bool initialState) => writer.Write(Value);
		protected override void Deserialize(NetworkReader reader, bool initialState) => Value = reader.Read<Vector3>();
	}
}
