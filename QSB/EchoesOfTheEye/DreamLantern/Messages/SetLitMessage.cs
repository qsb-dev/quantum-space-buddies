using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

public class SetLitMessage : QSBWorldObjectMessage<QSBDreamLanternController, bool>
{
	public SetLitMessage(bool lit) : base(lit) { }

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject.SetLit(Data);
		WorldObject.DreamLanternItem?._oneShotSource?.PlayOneShot(Data ? AudioType.Artifact_Light : AudioType.Artifact_Extinguish);

		// If a lantern is already lit you shouldn't be able to pick it up
		if (Data)
		{
			WorldObject.DreamLanternItem?.EnableInteraction(false);
		}
	}
}
