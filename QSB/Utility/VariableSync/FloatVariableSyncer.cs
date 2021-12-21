using QuantumUNET.Transport;

namespace QSB.Utility.VariableSync
{
	public class FloatVariableSyncer : BaseVariableSyncer<float>
	{
		protected override void WriteValue(QNetworkWriter writer, float value) => writer.Write(value);
		protected override float ReadValue(QNetworkReader reader) => reader.ReadSingle();
	}
}
