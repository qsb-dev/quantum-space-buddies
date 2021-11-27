namespace QSB.Events
{
	public interface IQSBEvent
	{
		EventType Type { get; }

		void SetupListener();
		void CloseListener();
	}
}