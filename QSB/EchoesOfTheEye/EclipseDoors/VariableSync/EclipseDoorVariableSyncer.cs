using QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseDoors.VariableSync;

public class EclipseDoorVariableSyncer : RotatingElementsVariableSyncer<QSBEclipseDoorController>
{
	protected override Transform[] RotatingElements => WorldObject.AttachedObject._rotatingElements;
}
