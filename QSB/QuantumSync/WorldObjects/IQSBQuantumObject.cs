using QSB.Player;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.QuantumSync.WorldObjects;

public interface IQSBQuantumObject : IWorldObject
{
	/// <summary>
	/// whether the controlling player is always the host <br/>
	/// also means this object is considered always enabled
	/// </summary>
	bool HostControls { get; }
	uint ControllingPlayer { get; set; }
	bool IsEnabled { get; }

	List<Shape> GetAttachedShapes();

	void SetIsQuantum(bool isQuantum);
	VisibilityObject GetVisibilityObject();
	void OnTakeProbeSnapshot(PlayerInfo player, ProbeCamera.ID cameraId);
	void OnRemoveProbeSnapshot(PlayerInfo player);
	List<PlayerInfo> GetVisibleToProbePlayers();
}