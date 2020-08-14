namespace QSB.Tools
{
    public static class ToolExtensions
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
