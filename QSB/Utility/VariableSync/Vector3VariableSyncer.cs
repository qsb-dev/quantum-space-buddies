using UnityEngine;

namespace QSB.Utility.VariableSync;

[UsedInUnityProject]
public class Vector3VariableSyncer : BaseVariableSyncer<Vector3>
{
	/// <summary>
	/// hack for ShipThrusterVariableSyncer
	/// </summary>
	public bool public_HasChanged() => HasChanged();
}