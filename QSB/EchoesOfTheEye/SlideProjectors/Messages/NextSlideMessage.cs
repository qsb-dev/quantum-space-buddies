using QSB.EchoesOfTheEye.SlideProjectors.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.SlideProjectors.Messages;

internal class NextSlideMessage : QSBWorldObjectMessage<QSBSlideProjector>
{
	public override void OnReceiveRemote() => WorldObject.AttachedObject.NextSlide();
}
