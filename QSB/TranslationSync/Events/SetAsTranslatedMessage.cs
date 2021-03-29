using QSB.WorldSync.Events;
using QuantumUNET.Transport;

namespace QSB.TranslationSync.Events
{
	public class SetAsTranslatedMessage : EnumWorldObjectMessage<NomaiTextType>
	{
		public int TextId { get; set; }

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			TextId = reader.ReadInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TextId);
		}
	}
}
