using QSB.Messaging;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;
using QSB.WorldSync;
using QuantumUNET.Transport;

namespace QSB.Tools.TranslatorTool.TranslationSync.Messages
{
	internal abstract class SetAsTranslatedMessage<T> : QSBWorldObjectMessage<T> where T : IWorldObject
	{
		protected int TextId;

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
	}

	internal class WallTextTranslatedMessage : SetAsTranslatedMessage<QSBWallText>
	{
		public WallTextTranslatedMessage(int textId) => TextId = textId;
		public override void OnReceiveRemote() => WorldObject.HandleSetAsTranslated(TextId);
	}

	internal class ComputerTranslatedMessage : SetAsTranslatedMessage<QSBComputer>
	{
		public ComputerTranslatedMessage(int textId) => TextId = textId;
		public override void OnReceiveRemote() => WorldObject.HandleSetAsTranslated(TextId);
	}

	internal class VesselComputerTranslatedMessage : SetAsTranslatedMessage<QSBVesselComputer>
	{
		public VesselComputerTranslatedMessage(int textId) => TextId = textId;
		public override void OnReceiveRemote() => WorldObject.HandleSetAsTranslated(TextId);
	}
}