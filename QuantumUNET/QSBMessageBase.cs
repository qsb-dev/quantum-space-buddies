namespace QuantumUNET
{
	public abstract class QSBMessageBase
	{
		public virtual void Deserialize(QSBNetworkReader reader)
		{
		}

		public virtual void Serialize(QSBNetworkWriter writer)
		{
		}
	}
}