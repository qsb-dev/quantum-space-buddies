using QuantumUNET.Transport;

namespace QSB.Utility.VariableSync
{
	public class BoolVariableSyncer : BaseVariableSyncer
	{
		public VariableReference<bool> FloatToSync;

		public override void WriteData(QNetworkWriter writer)
		{
			if (FloatToSync == null)
			{
				writer.Write(false);
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
				writer.ReadBoolean();
			}
			else
			{
				FloatToSync.Value = writer.ReadBoolean();
			}
		}

		public override bool HasChanged()
		{
			// TODO - do this!!
			return true;
		}
	}
}
