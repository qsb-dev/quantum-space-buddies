using QSB.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.HUD.Messages;

internal class ChatMessage : QSBMessage<string>
{
	public ChatMessage(string msg) : base(msg) { }

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		MultiplayerHUDManager.Instance.WriteMessage(Data);
	}
}