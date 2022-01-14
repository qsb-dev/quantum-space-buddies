using Mirror;
using QSB.Messaging;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;

namespace QSB.Tools.TranslatorTool.TranslationSync.Messages
{
	internal class SetAsTranslatedMessage : QSBWorldObjectMessage<QSBNomaiText>
	{
		private int TextId;

		public SetAsTranslatedMessage(int textId) => TextId = textId;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(TextId);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			TextId = reader.Read<int>();
		}

		public override void OnReceiveRemote() => WorldObject.SetAsTranslated(TextId);
	}
}