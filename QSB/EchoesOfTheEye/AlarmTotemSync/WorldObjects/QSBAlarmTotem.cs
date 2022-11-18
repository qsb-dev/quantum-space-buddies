using QSB.AuthoritySync;
using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

/// <summary>
/// TODO: make this not NRE (by not doing enable sync) and then readd it back in
/// </summary>
public class QSBAlarmTotem : AuthWorldObject<AlarmTotem>
{
	public override bool CanOwn => AttachedObject.enabled;

	public override void SendInitialState(uint to)
	{
		base.SendInitialState(to);

		this.SendMessage(new SetVisibleMessage(AttachedObject._isPlayerVisible) { To = to });
	}

	/// <summary>
	/// i love copy pasting
	/// </summary>
	public void SetVisible(bool isPlayerVisible)
	{
		if (!isPlayerVisible && AttachedObject._isPlayerVisible)
		{
			Locator.GetAlarmSequenceController().IncreaseAlarmCounter();
			AttachedObject._secondsConcealed = 0f;
			AttachedObject._simTotemMaterials[0] = AttachedObject._simAlarmMaterial;
			AttachedObject._simTotemRenderer.sharedMaterials = AttachedObject._simTotemMaterials;
			AttachedObject._simVisionConeRenderer.SetColor(AttachedObject._simAlarmColor);
			GlobalMessenger.FireEvent("AlarmTotemTriggered");
		}
		else if (isPlayerVisible && !AttachedObject._isPlayerVisible)
		{
			Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			AttachedObject._secondsConcealed = 0f;
			AttachedObject._simTotemMaterials[0] = AttachedObject._origSimEyeMaterial;
			AttachedObject._simTotemRenderer.sharedMaterials = AttachedObject._simTotemMaterials;
			AttachedObject._simVisionConeRenderer.SetColor(AttachedObject._simVisionConeRenderer.GetOriginalColor());
			AttachedObject._pulseLightController.FadeTo(0f, 0.5f);
		}
	}
}
