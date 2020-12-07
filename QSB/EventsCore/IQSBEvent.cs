namespace QSB.EventsCore
{
	public interface IQSBEvent
	{
		void SetupListener();

		void CloseListener();
	}
}