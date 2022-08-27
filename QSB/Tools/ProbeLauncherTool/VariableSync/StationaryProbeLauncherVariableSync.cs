using Mirror;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.Utility.LinkedWorldObject;
using QSB.Utility.VariableSync;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Tools.ProbeLauncherTool.VariableSync;

public class StationaryProbeLauncherVariableSync : BaseVariableSyncer<(float, float, float)>, ILinkedNetworkBehaviour
{
	protected override bool HasChanged()
	{
		var launcher = (StationaryProbeLauncher)WorldObject.AttachedObject;

		Value = (launcher._degreesX, launcher._degreesY, launcher._audioSource._localVolume);

		return Value != PrevValue;
	}

	protected override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);

		var launcher = (StationaryProbeLauncher)WorldObject.AttachedObject;

		writer.Write(launcher._degreesX);
		writer.Write(launcher._degreesY);
		writer.Write(launcher._audioSource.GetLocalVolume());
	}

	protected override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);

		var launcher = (StationaryProbeLauncher)WorldObject.AttachedObject;

		launcher._degreesX = reader.Read<float>();
		launcher._degreesY = reader.Read<float>();
		launcher._audioSource.SetLocalVolume(reader.Read<float>());

		// Update rotation based on x and y degrees
		launcher.transform.localRotation = Quaternion.AngleAxis(launcher._degreesX, launcher._localUpAxis) * launcher._initRotX;
		launcher._verticalPivot.localRotation = Quaternion.AngleAxis(launcher._degreesY, -Vector3.right) * launcher._initRotY;

		Value = (launcher._degreesX, launcher._degreesY, launcher._audioSource._localVolume);
	}

	protected QSBStationaryProbeLauncher WorldObject { get; private set; }
	public void SetWorldObject(IWorldObject worldObject) => WorldObject = (QSBStationaryProbeLauncher)worldObject;
}
