using QSB.Messaging;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.Tools.TranslatorTool.TranslationSync.Messages
{
	internal class SetAsTranslatedMessage : QSBWorldObjectMessage<QSBNomaiText>
	{
		private int TextId;

		public SetAsTranslatedMessage(int textId) => TextId = textId;

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TextId);
		}

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			TextId = reader.ReadInt32();
		}

		public override void OnReceiveRemote() => WorldObject.SetAsTranslated(TextId);
	}
}