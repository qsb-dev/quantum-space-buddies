using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using System.Collections.Generic;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class TotemVisibleForMessage : QSBWorldObjectMessage<QSBAlarmTotem, List<uint>>
{
	public TotemVisibleForMessage(List<uint> visibleFor) : base(visibleFor) { }

	public override void OnReceiveRemote()
	{
		WorldObject.VisibleFor.Clear();
		WorldObject.VisibleFor.AddRange(Data);
	}
}
