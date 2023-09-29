using QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers.Messages;

public class MoveSelectorMessage : QSBWorldObjectMessage<QSBEclipseCodeController, (int newSelectedDial, bool up)>
{
	public MoveSelectorMessage(int selectedDial, bool up) : base((selectedDial, up)) { }

	public override void OnReceiveRemote()
	{
		WorldObject.AttachedObject._selectedDial = Data.newSelectedDial;
		if (WorldObject.AttachedObject.MoveSelectorToLocalPositionY(WorldObject.AttachedObject._dials[Data.newSelectedDial].transform.localPosition.y))
		{
			WorldObject.AttachedObject._gearInterfaceVertical.AddRotation(Data.up ? 45f : -45f, 0f);
		}
		else
		{
			WorldObject.AttachedObject._gearInterfaceVertical.PlayFailure(Data.up, 0.5f);
		}
	}
}
