using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class SetFaceOpenMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public SetFaceOpenMessage(bool open) : base(open) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetFaceOpen(Data));
}
