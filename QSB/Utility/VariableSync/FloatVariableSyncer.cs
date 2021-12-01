using QuantumUNET.Transport;
using System;

namespace QSB.Utility.VariableSync
{
	public class FloatVariableSyncer : BaseVariableSyncer
	{
		public VariableReference<float> ValueToSync { get; private set; }

		public void Init(Func<float> getter, Action<float> setter)
		{
			ValueToSync = new(this);
			ValueToSync.Getter = getter;
			ValueToSync.Setter = setter;
			_ready = true;
		}

		public void OnDestroy()
			=> _ready = false;

		public override void WriteData(QNetworkWriter writer)
		{
			if (Ready)
			{
				writer.Write(ValueToSync.Value);
			}
			else
			{
				writer.Write(default(float));
			}
		}

		public override void ReadData(QNetworkReader reader)
		{
			if (Ready)
			{
				ValueToSync.Value = reader.ReadSingle();
			}
			else
			{
				reader.ReadSingle();
			}
		}
	}
}
