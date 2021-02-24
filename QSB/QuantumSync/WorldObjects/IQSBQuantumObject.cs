using QSB.WorldSync;

namespace QSB.QuantumSync
{
	public interface IQSBQuantumObject : IWorldObjectTypeSubset
	{
		uint ControllingPlayer { get; set; }
		bool IsEnabled { get; set; }
	}
}
