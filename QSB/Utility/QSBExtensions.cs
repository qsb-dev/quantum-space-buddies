namespace QSB.Utility
{
    public static class QSBExtensions
    {
        public static void ChangeEquipState(this PlayerTool tool, bool equipState)
        {
            if (equipState)
            {
                tool.EquipTool();
            }
            else
            {
                tool.UnequipTool();
            }
        }
    }
}
