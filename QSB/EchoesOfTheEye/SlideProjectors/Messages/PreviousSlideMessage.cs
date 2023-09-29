using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.SlideProjectors.Messages;

public class PreviousSlideMessage : QSBWorldObjectMessage<QSBSlideProjector>
{
	public override void OnReceiveRemote() => WorldObject.AttachedObject.PreviousSlide();
}
