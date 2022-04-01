using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class SetEnabledMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public SetEnabledMessage(bool data) : base(data) { }

	public override void OnReceiveRemote()
	{
		if (!Data && )

		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetFaceOpen(Data));
	}
}
