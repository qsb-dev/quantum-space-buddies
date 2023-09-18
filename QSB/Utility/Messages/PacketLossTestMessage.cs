using QSB.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.Utility.Messages;

internal class PacketLossTestMessage : QSBMessage
{
	public override void OnReceiveLocal()
	{
		DebugActions.TotalMessages += 1;
		DebugLog.DebugWrite($"Total test messages sent is now {DebugActions.TotalMessages}");
	}

	public override void OnReceiveRemote()
	{
		DebugActions.TotalMessages += 1;
		DebugLog.DebugWrite($"Total test messages recieved is now {DebugActions.TotalMessages}");
	}
}
