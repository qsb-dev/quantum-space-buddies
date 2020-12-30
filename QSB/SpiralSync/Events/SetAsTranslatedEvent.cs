using QSB.Events;
using QSB.SpiralSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.SpiralSync.Events
{
	public class SetAsTranslatedEvent : QSBEvent<SetAsTranslatedMessage>
	{
		public override EventType Type => EventType.TextTranslated;

		public override void SetupListener() => GlobalMessenger<NomaiTextType, int, int>.AddListener(EventNames.QSBTextTranslated, Handler);
		public override void CloseListener() => GlobalMessenger<NomaiTextType, int, int>.RemoveListener(EventNames.QSBTextTranslated, Handler);

		private void Handler(NomaiTextType type, int objId, int textId) => SendEvent(CreateMessage(type, objId, textId));

		private SetAsTranslatedMessage CreateMessage(NomaiTextType type, int objId, int textId) => new SetAsTranslatedMessage
		{
			AboutId = LocalPlayerId,
			ObjectId = objId,
			TextId = textId,
			TextType = type
		};

		public override void OnReceiveRemote(bool server, SetAsTranslatedMessage message)
		{
			if (!QSBCore.HasWokenUp)
			{
				return;
			}
			if (message.TextType == NomaiTextType.WallText)
			{
				var obj = QSBWorldSync.GetWorldObject<QSBWallText>(message.ObjectId);
				obj.HandleSetAsTranslated(message.TextId);
			}
			else
			{
				throw new System.NotImplementedException($"TextType <{message.TextType}> not implemented.");
			}
		}
	}
}