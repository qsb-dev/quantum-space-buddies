using Mirror;
using System;

namespace QSB.Utility.VariableSync
{
	public class BoolVariableSyncer : BaseVariableSyncer
	{
		private bool _prevValue;
		[NonSerialized]
		public bool Value;

		protected override bool HasChanged() => Value != _prevValue;
		protected override void UpdatePrevData() => _prevValue = Value;
		protected override void Serialize(NetworkWriter writer) => writer.Write(Value);
		protected override void Deserialize(NetworkReader reader) => Value = reader.Read<bool>();
	}
}
