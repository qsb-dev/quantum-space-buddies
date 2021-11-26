using QuantumUNET.Transport;
using System;

namespace QSB.Utility.VariableSync
{
	public class FloatVariableSyncer : BaseVariableSyncer
	{
		public VariableReference<float> ValueToSync { get; private set; } = new();

		public void Init(Func<float> getter, Action<float> setter)
		{
			ValueToSync.Getter = getter;
			ValueToSync.Setter = setter;
			_ready = true;
		}

		public void OnDestroy()
			=> _ready = false;

		public override void WriteData(QNetworkWriter writer)
			=> writer.Write(ValueToSync.Value);

		public override void ReadData(QNetworkReader writer)
			=> ValueToSync.Value = writer.ReadSingle();
	}
}
