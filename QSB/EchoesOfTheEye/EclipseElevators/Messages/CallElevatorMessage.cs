using QSB.EchoesOfTheEye.EclipseElevators.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.EclipseElevators.Messages;

public class CallElevatorMessage : QSBWorldObjectMessage<QSBElevatorDestination>
{
	public override void OnReceiveRemote()
	{
		if (WorldObject.AttachedObject._gearInterface != null)
		{
			if (WorldObject.AttachedObject._elevator.currentLevelIdx != WorldObject.AttachedObject._floorIndex || WorldObject.AttachedObject._elevator.isMoving)
			{
				WorldObject.AttachedObject._gearInterface.AddRotation(90f, 1f);
			}
			else
			{
				WorldObject.AttachedObject._gearInterface.PlayFailure(true, 1f);
			}
		}
		WorldObject.AttachedObject.OnElevatorCalled.Invoke(WorldObject.AttachedObject._floorIndex);
		WorldObject.AttachedObject._interactReceiver.ResetInteraction();
	}
}
