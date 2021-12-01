using QSB.Messaging;
using QuantumUNET.Components;
using QuantumUNET.Transport;

namespace QSB.SuspensionAuthoritySync
{
	public class SuspensionChangeMessage : PlayerMessage
	{
		public QNetworkIdentity Identity;
		public bool Suspended;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Identity = reader.ReadNetworkIdentity();
			Suspended = reader.ReadBoolean();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Identity);
			writer.Write(Suspended);
		}
	}
}
