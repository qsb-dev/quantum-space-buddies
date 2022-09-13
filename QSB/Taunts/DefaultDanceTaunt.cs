using QSB.Taunts.ThirdPersonCamera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.Taunts;

internal class DefaultDanceTaunt : ITaunt
{
	public bool Loops => false;
	public TauntBodyGroup BodyGroup => TauntBodyGroup.WholeBody;
	public string TriggerName => "DefaultDance";
	public string ClipName => "DanceMoves";
	public string StateName => "Default Dance";
	public CameraMode CameraMode => CameraMode.ThirdPerson;
}
