using Mirror;
using QuantumUNET.Transport;
using System;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public class Vector3VariableSyncer2 : BaseVariableSyncer2
	{
		private Vector3 _prevValue;
		[NonSerialized]
		public Vector3 Value;

		protected override bool HasChanged() => Value != _prevValue;
		protected override void UpdatePrevData() => _prevValue = Value;
		protected override void Serialize(NetworkWriter writer, bool initialState) => writer.Write(Value);
		protected override void Deserialize(NetworkReader reader, bool initialState) => Value = reader.Read<Vector3>();
	}

	public class Vector3VariableSyncer : BaseVariableSyncer<Vector3>
	{
		protected override void WriteValue(QNetworkWriter writer, Vector3 value) => writer.Write(value);
		protected override Vector3 ReadValue(QNetworkReader reader) => reader.ReadVector3();
	}
}
