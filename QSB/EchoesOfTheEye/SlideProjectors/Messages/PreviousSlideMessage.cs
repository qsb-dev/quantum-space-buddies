using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.SlideProjectors.Messages;

internal class PreviousSlideMessage : QSBWorldObjectMessage<QSBSlideProjector>
{
	public override void OnReceiveRemote() => QSBPatch.RemoteCall(WorldObject.AttachedObject.PreviousSlide);
}
