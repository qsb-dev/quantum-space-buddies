using QSB.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.HUD.Messages;

public class ChatMessage : QSBMessage<(string message, Color color)>
{
	public ChatMessage(string msg, Color color) : base((msg, color)) { }

	public override void OnReceiveLocal() => OnReceiveRemote();

	public override void OnReceiveRemote()
	{
		MultiplayerHUDManager.Instance.WriteMessage(Data.message, Data.color);
	}
}