using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using System.Collections.Generic;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class VisibleForMessage : QSBWorldObjectMessage<QSBAlarmTotem, List<uint>>
{
	public VisibleForMessage(List<uint> visibleFor) : base(visibleFor) { }

	public override void OnReceiveRemote()
	{
		WorldObject.VisibleFor.Clear();
		WorldObject.VisibleFor.AddRange(Data);
	}
}
