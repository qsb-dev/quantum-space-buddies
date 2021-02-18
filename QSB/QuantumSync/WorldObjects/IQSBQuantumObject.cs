namespace QSB.QuantumSync
{
	public interface IQSBQuantumObject
	{
		uint ControllingPlayer { get; set; }
		bool IsEnabled { get; set; }

		void Enable();
		void Disable();
	}
}
