using QSB.Messaging;
using QSB.Player;

namespace QSB.Tools.TranslatorTool.Messages;

public class IsTranslatingMessage : QSBMessage<bool>
{
	public IsTranslatingMessage(bool translating) : base(translating) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		player.IsTranslating = Data;
		((QSBNomaiTranslator)player.Translator).UpdateTranslating(Data);
	}

	public override void OnReceiveLocal()
		=> QSBPlayerManager.LocalPlayer.IsTranslating = Data;
}
