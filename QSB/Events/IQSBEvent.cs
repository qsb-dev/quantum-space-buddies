namespace QSB.Events
{
    public interface IQSBEvent
    {
        void SetupListener();
        void CloseListener();
    }
}
