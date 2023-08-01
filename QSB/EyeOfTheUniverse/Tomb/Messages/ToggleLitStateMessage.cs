using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.Tomb.Messages;

public class ToggleLitStateMessage : QSBMessage<(int stateIndex, int direction, bool wasLit)>
{
	public ToggleLitStateMessage(int currentStateIndex, int direction, bool wasLit) : base((currentStateIndex, direction, wasLit)) { }

	public override void OnReceiveRemote()
	{
		var tomb = QSBWorldSync.GetUnityObject<EyeTombController>();

		if (tomb._stateIndex != Data.stateIndex)
		{
			DebugLog.ToConsole($"Warning - Received ToggleLitStateMessage with stateIndex of {Data.stateIndex}, but is currently {tomb._stateIndex}. Correcting...");
			tomb._states[tomb._stateIndex].SetActive(false);
			tomb._stateIndex = Data.stateIndex;
		}

		if (tomb._lit != Data.wasLit)
		{
			DebugLog.ToConsole($"Warning - Received ToggleLitStateMessage, and the value of lit did not match. Correcting...");
			tomb._lit = Data.wasLit;
		}

		tomb._lit = !tomb._lit;
		tomb._planetLightController.SetIntensity(tomb._lit ? 1f : 0f);
		tomb._planetObject.SetActive(tomb._lit);
		tomb._lightBeamController.SetFade(tomb._lit ? 1f : 0f);

		if (!tomb._lit)
		{
			tomb._states[tomb._stateIndex].SetActive(false);
			tomb._stateIndex += Data.direction;
			tomb._states[tomb._stateIndex].SetActive(true);
		}

		tomb._gearEffects.AddRotation(Data.direction * 45f, 0f);
		tomb._oneShotSource.PlayOneShot((Data.direction > 0f) ? AudioType.Projector_Next : AudioType.Projector_Prev, 1f);
	}
}
