using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public class Vector3VariableSyncer : BaseVariableSyncer<Vector3>
	{
		protected override void WriteValue(QNetworkWriter writer, Vector3 value) => writer.Write(value);
		protected override Vector3 ReadValue(QNetworkReader reader) => reader.ReadVector3();
	}
}
