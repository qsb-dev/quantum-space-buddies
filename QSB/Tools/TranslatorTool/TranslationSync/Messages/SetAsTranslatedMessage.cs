using QSB.Messaging;
using QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;

namespace QSB.Tools.TranslatorTool.TranslationSync.Messages;

public class SetAsTranslatedMessage : QSBWorldObjectMessage<QSBNomaiText, int>
{
	public SetAsTranslatedMessage(int textId) : base(textId) { }

	public override void OnReceiveRemote() => WorldObject.SetAsTranslated(Data);
}