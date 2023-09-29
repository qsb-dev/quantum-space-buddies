using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner.WorldObjects;

public class QSBPrisonerMarker : WorldObject<PrisonerBehaviourCueMarker>
{
	public Transform Transform => AttachedObject.transform;
}
