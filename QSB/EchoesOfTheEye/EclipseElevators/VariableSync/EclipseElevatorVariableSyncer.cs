using QSB.EchoesOfTheEye.EclipseElevators.WorldObjects;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseElevators.VariableSync;

public class EclipseElevatorVariableSyncer : RotatingElementsVariableSyncer<QSBEclipseElevatorController>
{
	protected override Transform[] RotatingElements => WorldObject.AttachedObject._rotatingElements;
}
