using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class SetVisibleMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public SetVisibleMessage(bool visible) : base(visible) { }

	public override void OnReceiveRemote()
	{
		if (WorldObject.AttachedObject._isPlayerVisible == Data)
		{
			return;
		}

		WorldObject.AttachedObject._isPlayerVisible = Data;
		if (Data)
		{
			Locator.GetAlarmSequenceController().IncreaseAlarmCounter();
			WorldObject.AttachedObject._secondsConcealed = 0f;
			WorldObject.AttachedObject._simTotemMaterials[0] = WorldObject.AttachedObject._simAlarmMaterial;
			WorldObject.AttachedObject._simTotemRenderer.sharedMaterials = WorldObject.AttachedObject._simTotemMaterials;
			WorldObject.AttachedObject._simVisionConeRenderer.SetColor(WorldObject.AttachedObject._simAlarmColor);
			GlobalMessenger.FireEvent("AlarmTotemTriggered");
		}
		else
		{
			Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			WorldObject.AttachedObject._secondsConcealed = 0f;
			WorldObject.AttachedObject._simTotemMaterials[0] = WorldObject.AttachedObject._origSimEyeMaterial;
			WorldObject.AttachedObject._simTotemRenderer.sharedMaterials = WorldObject.AttachedObject._simTotemMaterials;
			WorldObject.AttachedObject._simVisionConeRenderer.SetColor(WorldObject.AttachedObject._simVisionConeRenderer.GetOriginalColor());
			WorldObject.AttachedObject._pulseLightController.FadeTo(0f, 0.5f);
		}
	}
}
