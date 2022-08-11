﻿using QSB.Animation.NPC.WorldObjects;
using QSB.Messaging;

namespace QSB.Animation.NPC.Messages;

//TODO Remove this
internal class CharacterAnimControllerMessage : QSBWorldObjectMessage<QSBCharacterAnimController, bool>
{
	public CharacterAnimControllerMessage(bool isInConversation) : base(isInConversation) { }

	public override void OnReceiveRemote() => WorldObject.SetInConversation(Data);
}