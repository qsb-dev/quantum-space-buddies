using Mirror;
using System;

namespace QSB.Utility.VariableSync
{
	public class FloatVariableSyncer : BaseVariableSyncer
	{
		private float _prevValue;
		[NonSerialized]
		public float Value;

		protected override bool HasChanged() => Value != _prevValue;
		protected override void UpdatePrevData() => _prevValue = Value;
		protected override void Serialize(NetworkWriter writer, bool initialState) => writer.Write(Value);
		protected override void Deserialize(NetworkReader reader, bool initialState) => Value = reader.Read<float>();
	}
}
