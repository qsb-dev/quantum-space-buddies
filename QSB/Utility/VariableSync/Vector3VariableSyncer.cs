using QuantumUNET.Transport;
using UnityEngine;

namespace QSB.Utility.VariableSync
{
	public class Vector3VariableSyncer : BaseVariableSyncer
	{
		public VariableReference<Vector3> FloatToSync;

		public override void WriteData(QNetworkWriter writer)
		{
			if (FloatToSync == null)
			{
				writer.Write(Vector3.zero);
			}
			else
			{
				writer.Write(FloatToSync.Value);
			}
		}

		public override void ReadData(QNetworkReader writer)
		{
			if (FloatToSync == null)
			{
				writer.ReadVector3();
			}
			else
			{
				FloatToSync.Value = writer.ReadVector3();
			}
		}

		public override bool HasChanged()
		{
			// TODO - do this!!
			return true;
		}
	}
}
