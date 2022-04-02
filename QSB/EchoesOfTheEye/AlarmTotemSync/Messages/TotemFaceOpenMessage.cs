using QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.Messages;

public class TotemFaceOpenMessage : QSBWorldObjectMessage<QSBAlarmTotem, bool>
{
	public TotemFaceOpenMessage(bool open) : base(open) { }

	public override void OnReceiveRemote() =>
		QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetFaceOpen(Data));
}
