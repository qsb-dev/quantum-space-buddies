using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using QSB.Tools.ProbeTool;
using QSB.Tools.ProbeTool.Messages;
using UnityEngine;

namespace QSB.Player;

public class ProbeCloakWatcher : MonoBehaviour, IAddComponentOnStart
{
	private bool _probeWasInCloak;

	public void Update()
	{
		if (!QSBCore.IsInMultiplayer)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		var fieldController = Locator.GetCloakFieldController();
		if (fieldController == null)
		{
			return;
		}

		var probeInCloak = fieldController._probeInsideCloak;

		if (probeInCloak == _probeWasInCloak)
		{
			return;
		}

		if (probeInCloak)
		{
			new ProbeEnterLeaveMessage(ProbeEnterLeaveType.EnterCloak).Send();
		}
		else
		{
			new ProbeEnterLeaveMessage(ProbeEnterLeaveType.ExitCloak).Send();
		}

		_probeWasInCloak = probeInCloak;
	}
}
