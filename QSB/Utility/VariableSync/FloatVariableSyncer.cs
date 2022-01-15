using Mirror;
using QuantumUNET.Transport;
using System;

namespace QSB.Utility.VariableSync
{
	public class FloatVariableSyncer2 : BaseVariableSyncer2
	{
		private float _prevValue;
		[NonSerialized]
		public float Value;

		protected override bool HasChanged() => Value != _prevValue;
		protected override void UpdatePrevData() => _prevValue = Value;
		protected override void Serialize(NetworkWriter writer, bool initialState) => writer.Write(Value);
		protected override void Deserialize(NetworkReader reader, bool initialState) => Value = reader.Read<float>();
	}

	public class FloatVariableSyncer : BaseVariableSyncer<float>
	{
		protected override void WriteValue(QNetworkWriter writer, float value) => writer.Write(value);
		protected override float ReadValue(QNetworkReader reader) => reader.ReadSingle();
	}
}
