using QSB.EchoesOfTheEye.EclipseCodeControllers.WorldObjects;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.EchoesOfTheEye.EclipseCodeControllers.Messages;

public class RotateDialMessage : QSBWorldObjectMessage<QSBEclipseCodeController, (bool right, int selectedDial)>
{
	public RotateDialMessage(bool right, int selectedDial) : base((right, selectedDial)) { }

	public override void OnReceiveRemote()
	{
		if (WorldObject.AttachedObject._selectedDial != Data.selectedDial)
		{
			DebugLog.ToConsole($"Warning - {WorldObject} got a RotateDialMessage, but it's _selectedDial is mismatched. Correcting...", OWML.Common.MessageType.Warning);
			WorldObject.AttachedObject._selectedDial = Data.selectedDial;
			WorldObject.AttachedObject.MoveSelectorToLocalPositionY(WorldObject.AttachedObject._dials[Data.selectedDial].transform.localPosition.y);
		}

		WorldObject.AttachedObject._dials[WorldObject.AttachedObject._selectedDial].Rotate(Data.right);
		WorldObject.AttachedObject._gearInterfaceHorizontal.AddRotation(Data.right ? -45f : 45f, 0f);
		WorldObject.AttachedObject._oneShotAudio.PlayOneShot(AudioType.CodeTotem_Horizontal, 1f);
		WorldObject.AttachedObject._codeCheckDirty = true;
	}
}
