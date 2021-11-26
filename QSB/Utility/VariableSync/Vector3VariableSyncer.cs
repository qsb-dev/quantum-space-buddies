using QuantumUNET.Transport;
using System;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public class Vector3VariableSyncer : BaseVariableSyncer
	{
		public VariableReference<Vector3> ValueToSync { get; private set; } = new();

		public void Init(Func<Vector3> getter, Action<Vector3> setter)
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
			=> ValueToSync.Value = writer.ReadVector3();
	}
}
