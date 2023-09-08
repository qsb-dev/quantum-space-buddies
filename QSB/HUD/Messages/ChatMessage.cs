using QSB.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QSB.Player;
using UnityEngine;

namespace QSB.HUD.Messages;

public class ChatMessage : QSBMessage<(string message, Color color)>
{
	public ChatMessage(string msg, Color color) : base((msg, color)) { }

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		MultiplayerHUDManager.Instance.WriteMessage(Data.message, Data.color);

		var fromPlayer = QSBPlayerManager.GetPlayer(From);
		var qsb = false;
		string name;
		if (Data.message.StartsWith("QSB: "))
		{
			name = "QSB: ";
			qsb = true;
		}
		else if (Data.message.StartsWith($"{fromPlayer.Name}: "))
		{
			name = $"{fromPlayer.Name}: ";
		}
		else
		{
			// uhhh idk what happened
			MultiplayerHUDManager.OnChatMessageEvent.Invoke(Data.message, From);
			return;
		}

		var messageWithoutName = Data.message.Remove(Data.message.IndexOf(name), name.Length);
		MultiplayerHUDManager.OnChatMessageEvent.Invoke(messageWithoutName, qsb ? uint.MaxValue : From);
	}
}