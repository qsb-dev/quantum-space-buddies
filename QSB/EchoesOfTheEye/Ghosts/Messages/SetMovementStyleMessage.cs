using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.Ghosts.Messages;

public class SetMovementStyleMessage : QSBWorldObjectMessage<QSBGhostEffects, GhostEffects.MovementStyle>
{
	public SetMovementStyleMessage(GhostEffects.MovementStyle style) : base(style) { }

	public override void OnReceiveRemote()
		=> WorldObject.SetMovementStyle(Data, true);
}
