using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.Prisoner.WorldObjects;

internal class QSBPrisonerMarker : WorldObject<PrisonerBehaviourCueMarker>
{
	public override void SendInitialState(uint to) { }

	public Transform Transform => AttachedObject.transform;
}
