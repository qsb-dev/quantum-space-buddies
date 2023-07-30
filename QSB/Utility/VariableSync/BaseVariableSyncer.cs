using Mirror;
using System;
using System.Collections.Generic;

namespace QSB.Utility.VariableSync;

public abstract class BaseVariableSyncer<T> : QSBNetworkBehaviour
{
	protected override float SendInterval => 0.1f;

	protected T PrevValue;
	[NonSerialized]
	public T Value;

	// DO NOT REMOVE THIS METHOD
	public bool Bruh() => HasChanged();
	protected override bool HasChanged() => !EqualityComparer<T>.Default.Equals(PrevValue, Value);
	protected override void UpdatePrevData() => PrevValue = Value;
	protected override void Serialize(NetworkWriter writer) => writer.Write(Value);
	protected override void Deserialize(NetworkReader reader) => Value = reader.Read<T>();
}