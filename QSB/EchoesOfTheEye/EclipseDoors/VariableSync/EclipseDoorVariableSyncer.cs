using QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseDoors.VariableSync;

internal class EclipseDoorVariableSyncer : RotatingElementsVariableSyncer<QSBEclipseDoorController>
{
	protected override Transform[] RotatingElements => WorldObject.AttachedObject._rotatingElements;
}
