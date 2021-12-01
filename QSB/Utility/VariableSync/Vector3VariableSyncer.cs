using QuantumUNET.Transport;
using System;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public class Vector3VariableSyncer : BaseVariableSyncer
	{
		public VariableReference<Vector3> ValueToSync { get; private set; }

		public void Init(Func<Vector3> getter, Action<Vector3> setter)
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
				writer.Write(default(Vector3));
			}
		}

		public override void ReadData(QNetworkReader reader)
		{
			if (Ready)
			{
				ValueToSync.Value = reader.ReadVector3();
			}
			else
			{
				reader.ReadVector3();
			}
		}
	}
}
