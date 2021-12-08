using QuantumUNET.Transport;

namespace QSB.Events
{
	public abstract class QSBMessage
	{
		public abstract void Serialize(QNetworkWriter writer);
		public abstract void Deserialize(QNetworkReader reader);

		public virtual bool ShouldReceive => true;
		public abstract void OnReceive(bool isLocal);
	}
}
