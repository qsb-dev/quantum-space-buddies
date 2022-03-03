using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.QuantumSync.WorldObjects;

public interface IQSBQuantumObject : IWorldObject
{
	uint ControllingPlayer { get; set; }
	bool IsEnabled { get; }

	List<Shape> GetAttachedShapes();

	void SetIsQuantum(bool isQuantum);
}