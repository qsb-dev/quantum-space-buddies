using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.TranslationSync.Events
{
	public class SetAsTranslatedMessage : WorldObjectMessage
	{
		public int TextId { get; set; }
		public NomaiTextType TextType { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			TextId = reader.ReadInt32();
			TextType = (NomaiTextType)reader.ReadInt16();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TextId);
			writer.Write((short)TextType);
		}
	}
}
