using Mirror;
using System;
using System.Collections.Generic;

namespace QSB.Utility.VariableSync
{
	public abstract class BaseVariableSyncer<T> : QSBNetworkBehaviour
	{
		protected override float SendInterval => 0.1f;

		private T _prevValue;
		[NonSerialized]
		public T Value;

		protected override bool HasChanged() => !EqualityComparer<T>.Default.Equals(_prevValue, Value);
		protected override void UpdatePrevData() => _prevValue = Value;
		protected override void Serialize(NetworkWriter writer, bool initialState) => writer.Write(Value);
		protected override void Deserialize(NetworkReader reader, bool initialState) => Value = reader.Read<T>();
	}
}
