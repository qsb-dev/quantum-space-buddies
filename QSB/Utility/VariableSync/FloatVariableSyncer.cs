using QuantumUNET;
using QuantumUNET.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.Utility.VariableSync
{
	internal class FloatVariableSyncer : BaseVariableSyncer
	{
		public VariableReference<float> FloatToSync;

		public override void WriteData(QNetworkWriter writer)
		{
			DebugLog.DebugWrite($"write data!");
			writer.Write(FloatToSync.Value);
		}

		public override void ReadData(QNetworkReader writer)
		{
			DebugLog.DebugWrite($"read data!");
			FloatToSync.Value = writer.ReadSingle();
		}

		public override bool HasChanged()
		{
			// TODO - do this!!
			return true;
		}
	}
}
