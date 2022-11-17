using QSB.AuthoritySync;

namespace QSB.EchoesOfTheEye.AlarmTotemSync.WorldObjects;

/// <summary>
/// TODO: make this not NRE (by not doing enable sync) and then readd it back in
/// </summary>
public class QSBAlarmTotem : AuthWorldObject<AlarmTotem>
{
	public override bool CanOwn => AttachedObject.enabled;
}
