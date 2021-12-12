using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.QuantumSync
{
	public interface IQSBQuantumObject : IWorldObject
	{
		uint ControllingPlayer { get; set; }
		bool IsEnabled { get; set; }

		List<ShapeVisibilityTracker> GetVisibilityTrackers();
		List<Shape> GetAttachedShapes();
	}
}
