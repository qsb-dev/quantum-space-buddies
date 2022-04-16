using QSB.Messaging;
using QSB.Player;
using QSB.ShipSync.Messages;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.ShipSync.WorldObjects;

internal class QSBShipLight : WorldObject<ShipLight>
{
	public override void SendInitialState(uint to)
	{
		// todo : add this
	}

	public void SetOn(bool on)
	{
		AttachedObject._on = on;
		AttachedObject._startIntensity = AttachedObject._light.intensity;
		AttachedObject._targetIntensity = (AttachedObject._on && !AttachedObject._damaged && AttachedObject._powered)
			? (AttachedObject._baseIntensity * AttachedObject._intensityScale)
			: 0f;
		AttachedObject._fadeStartTime = Time.time;
		if (AttachedObject._lightSourceVol != null)
		{
			AttachedObject._lightSourceVol.SetVolumeActivation(AttachedObject._on);
		}

		AttachedObject.enabled = true;
	}
}
