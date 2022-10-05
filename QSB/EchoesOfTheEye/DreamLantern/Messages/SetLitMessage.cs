using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using System.Collections.Generic;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetLitMessage : QSBWorldObjectMessage<QSBDreamLanternController, bool>
{
	public SetLitMessage(bool lit) : base(lit) { }

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject.SetLit(Data);
		WorldObject.DreamLanternItem?._oneShotSource?.PlayOneShot(Data ? AudioType.Artifact_Light : AudioType.Artifact_Extinguish, 1f);
	}
}
