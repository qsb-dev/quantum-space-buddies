using QSB.EchoesOfTheEye.AlarmTotemSync.Messages;
using QSB.Messaging;
using QSB.Player;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

public class QSBAlarmTotem : WorldObject<AlarmTotem>
{
	public readonly List<PlayerInfo> VisibleFor = new();

	public override void SendInitialState(uint to)
	{
		this.SendMessage(new SetFaceOpenMessage(AttachedObject._isFaceOpen) { To = to });
		this.SendMessage(new SetEnabledMessage(AttachedObject.enabled) { To = to });
	}

	public void SetEnabled(bool enabled)
	{
		if (AttachedObject.enabled == enabled)
		{
			return;
		}

		if (!enabled &&
			AttachedObject._sector &&
			AttachedObject._sector.ContainsOccupant(DynamicOccupant.Player))
		{
			// local player is in sector, do not disable
			return;
		}

		AttachedObject.enabled = enabled;

		if (!enabled)
		{
			AttachedObject._pulseLightController.SetIntensity(0f);
			AttachedObject._simTotemMaterials[0] = AttachedObject._origSimEyeMaterial;
			AttachedObject._simTotemRenderer.sharedMaterials = AttachedObject._simTotemMaterials;
			AttachedObject._simVisionConeRenderer.SetColor(AttachedObject._simVisionConeRenderer.GetOriginalColor());
			if (AttachedObject._isPlayerVisible)
			{
				AttachedObject._isPlayerVisible = false;
				Locator.GetAlarmSequenceController().DecreaseAlarmCounter();
			}
		}
	}
}
