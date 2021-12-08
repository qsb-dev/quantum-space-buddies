using QuantumUNET.Transport;
using System;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public class Vector3VariableSyncer : BaseVariableSyncer
	{
		private VariableReference<Vector3> _ref;
		private Vector3 _prevValue;

		public void Init(Func<Vector3> getter, Action<Vector3> setter)
		{
			_ref = new VariableReference<Vector3>(this, getter, setter);
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
				_ref.Value = reader.ReadVector3();
			}
			else
			{
				reader.ReadVector3();
			}
		}

		public override bool HasChanged() => _ref.Value != _prevValue;
	}
}
