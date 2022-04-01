using QSB.EchoesOfTheEye.LightSensorSync.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using System;

namespace QSB.EchoesOfTheEye.LightSensorSync.WorldObjects;

internal class QSBLightSensor : WorldObject<SingleLightSensor>
{
	public bool IlluminatedByLocal;

	public Action OnDetectLocalLight;
	public Action OnDetectLocalDarkness;

	public override void SendInitialState(uint to) =>
		this.SendMessage(new SetEnabledMessage(AttachedObject.enabled) { To = to });

	public void SetEnabled(bool enabled)
	{
		if (AttachedObject._sector && AttachedObject._sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
		{
			// local player is in sector, do not disable lights
			return;
		}

		if (enabled && !AttachedObject.enabled)
		{
			AttachedObject.enabled = true;
			AttachedObject._lightDetector.GetShape().enabled = true;
			if (AttachedObject._preserveStateWhileDisabled)
			{
				AttachedObject._fixedUpdateFrameDelayCount = 10;
			}
		}
		else if (!enabled && AttachedObject.enabled)
		{
			AttachedObject.enabled = false;
			AttachedObject._lightDetector.GetShape().enabled = false;
			if (!AttachedObject._preserveStateWhileDisabled)
			{
				if (AttachedObject._illuminated)
				{
					AttachedObject.OnDetectDarkness.Invoke();
				}

				AttachedObject._illuminated = false;
			}
		}
	}
}
