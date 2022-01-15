using Mirror;
using QuantumUNET.Transport;
using System;

namespace QSB.Utility.VariableSync
{
	public class BoolVariableSyncer2 : BaseVariableSyncer2
	{
		private bool _prevValue;
		[NonSerialized]
		public bool Value;

		protected override bool HasChanged() => Value != _prevValue;
		protected override void UpdatePrevValue() => _prevValue = Value;
		protected override void Serialize(NetworkWriter writer) => writer.Write(Value);
		protected override void Deserialize(NetworkReader reader) => Value = reader.Read<bool>();
	}

	public class BoolVariableSyncer : BaseVariableSyncer<bool>
	{
		protected override void WriteValue(QNetworkWriter writer, bool value) => writer.Write(value);
		protected override bool ReadValue(QNetworkReader reader) => reader.ReadBoolean();
	}
}
