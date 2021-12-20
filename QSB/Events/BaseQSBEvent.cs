namespace QSB.Events
{
	public abstract class BaseQSBEvent : IQSBEvent
	{
		internal static int _msgType;

		public abstract void SetupListener();
		public abstract void CloseListener();
	}
}
