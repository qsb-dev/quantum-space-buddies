﻿using QSB.Player;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.QuantumSync.WorldObjects;

public interface IQSBQuantumObject : IWorldObject
{
	bool HostControls { get; }
	uint ControllingPlayer { get; set; }
	bool IsEnabled { get; }

	List<Shape> GetAttachedShapes();

	void SetIsQuantum(bool isQuantum);
	VisibilityObject GetVisibilityObject();
	void OnTakeProbeSnapshot(PlayerInfo player, ProbeCamera.ID cameraId);
	void OnRemoveProbeSnapshot(PlayerInfo player);
}