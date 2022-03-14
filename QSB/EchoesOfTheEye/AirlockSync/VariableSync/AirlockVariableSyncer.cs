using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using UnityEngine;

namespace QSB.EchoesOfTheEye.AirlockSync.VariableSync;

internal class AirlockVariableSyncer : RotatingElementsVariableSyncer<QSBGhostAirlock>
{
	protected override Transform[] RotatingElements => WorldObject.AttachedObject._interface._rotatingElements;
}
