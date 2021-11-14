using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.MeteorSync.Events
{
	public class FragmentResyncMessage : WorldObjectMessage
	{
		public float Integrity;
		public float OrigIntegrity;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			Integrity = reader.ReadSingle();
			OrigIntegrity = reader.ReadSingle();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(Integrity);
			writer.Write(OrigIntegrity);
		}
	}
}
