using QSB.Messaging;
using QSB.Player.Messages;
using QSB.Player.TransformSync;
using QSB.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using QSB.Utility;

namespace QSB.Tools.ProbeTool.Messages;

public class ProbeEnterLeaveMessage : QSBMessage<ProbeEnterLeaveType>
{
	static ProbeEnterLeaveMessage()
	{
		GlobalMessenger.AddListener(OWEvents.ProbeEnterQuantumMoon, () => Handler(ProbeEnterLeaveType.EnterQuantumMoon));
		GlobalMessenger.AddListener(OWEvents.ProbeExitQuantumMoon, () => Handler(ProbeEnterLeaveType.ExitQuantumMoon));
		// TODO : add cloak messages
	}

	private static void Handler(ProbeEnterLeaveType type)
	{
		new ProbeEnterLeaveMessage(type).Send();
	}

	public ProbeEnterLeaveMessage(ProbeEnterLeaveType type) : base(type) { }

	public override void OnReceiveRemote()
	{
		var player = QSBPlayerManager.GetPlayer(From);
		switch (Data)
		{
			case ProbeEnterLeaveType.EnterQuantumMoon:
				DebugLog.DebugWrite($"{player} probe enter QM");
				player.Probe.InsideQuantumMoon = true;
				break;
			case ProbeEnterLeaveType.ExitQuantumMoon:
				DebugLog.DebugWrite($"{player} probe exit QM");
				player.Probe.InsideQuantumMoon = false;
				break;
		}
	}
}
