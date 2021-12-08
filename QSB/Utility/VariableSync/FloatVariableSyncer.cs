using QuantumUNET.Transport;
using System;

namespace QSB.Utility.VariableSync
{
	public class FloatVariableSyncer : BaseVariableSyncer
	{
		private VariableReference<float> _ref;
		private float _prevValue;

		public void Init(Func<float> getter, Action<float> setter)
		{
			_ref = new VariableReference<float>(this, getter, setter);
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
				writer.Write(default(float));
			}
		}

		public override void ReadData(QNetworkReader reader)
		{
			if (Ready)
			{
				_ref.Value = reader.ReadSingle();
			}
			else
			{
				reader.ReadSingle();
			}
		}

		public override bool HasChanged() => _ref.Value != _prevValue;
	}
}
