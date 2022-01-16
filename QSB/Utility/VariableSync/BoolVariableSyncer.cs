using QuantumUNET.Transport;

namespace QSB.Utility.VariableSync
{
	public class BoolVariableSyncer : BaseVariableSyncer<bool>
	{
		protected override void WriteValue(QNetworkWriter writer, bool value) => writer.Write(value);
		protected override bool ReadValue(QNetworkReader reader) => reader.ReadBoolean();
	}
}
