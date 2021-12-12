using QuantumUNET.Transport;
using System;

namespace QSB.Utility.VariableSync
{
	public class BoolVariableSyncer : BaseVariableSyncer
	{
		private VariableReference<bool> _ref;
		private bool _prevValue;

		public void Init(Func<bool> getter, Action<bool> setter)
		{
			_ref = new VariableReference<bool>(this, getter, setter);
			Ready = true;
		}

		public override void WriteData(QNetworkWriter writer)
		{
			if (Ready)
			{
				writer.Write(_ref.Value);
				_prevValue = _ref.Value;
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
				_ref.Value = reader.ReadBoolean();
			}
			else
			{
				reader.ReadBoolean();
			}
		}

		public override bool HasChanged() => _ref.Value != _prevValue;
	}
}
