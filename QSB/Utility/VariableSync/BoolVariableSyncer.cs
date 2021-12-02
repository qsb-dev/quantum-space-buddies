using QuantumUNET.Transport;
using System;

namespace QSB.Utility.VariableSync
{
	public class BoolVariableSyncer : BaseVariableSyncer
	{
		public VariableReference<bool> ValueToSync { get; private set; }

		public void Init(Func<bool> getter, Action<bool> setter)
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
				writer.Write(default(bool));
			}
		}

		public override void ReadData(QNetworkReader reader)
		{
			if (Ready)
			{
				ValueToSync.Value = reader.ReadBoolean();
			}
			else
			{
				reader.ReadBoolean();
			}
		}
	}
}
