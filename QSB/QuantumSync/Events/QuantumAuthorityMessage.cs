using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.QuantumSync.Events
{
	public class QuantumAuthorityMessage : WorldObjectMessage
	{
		public uint AuthorityOwner { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			AuthorityOwner = reader.ReadUInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(AuthorityOwner);
		}
	}
}