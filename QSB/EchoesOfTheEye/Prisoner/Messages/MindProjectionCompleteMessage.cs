﻿using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

internal class MindProjectionCompleteMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();
		director.OnMindProjectionComplete();
	}
}
