using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using System.Collections.Generic;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

/// <summary>
/// sent by host on initial state
/// </summary>
public class VisibleForMessage : QSBWorldObjectMessage<QSBAlarmTotem, List<uint>>
{
	public VisibleForMessage(List<uint> visibleFor) : base(visibleFor) { }

	public override void OnReceiveRemote()
	{
		WorldObject.VisibleFor.Clear();
		WorldObject.VisibleFor.AddRange(Data);
		WorldObject.UpdateVisible();
	}
}
