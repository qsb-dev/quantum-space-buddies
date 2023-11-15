using QSB.OwnershipSync;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

public class QSBAlarmTotem : OwnedWorldObject<AlarmTotem>
{
	public override bool CanOwn => AttachedObject.enabled;
}
