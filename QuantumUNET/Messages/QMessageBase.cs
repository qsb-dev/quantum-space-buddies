using QuantumUNET.Transport;

namespace QuantumUNET.Messages
{
	public abstract class QMessageBase
	{
		public virtual void Serialize(QNetworkWriter writer) { }

		public virtual void Deserialize(QNetworkReader reader) { }
	}
}