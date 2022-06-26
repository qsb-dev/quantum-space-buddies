using System.Linq;

namespace QSB.Utility;

internal static class UIHelper
{
	public static void ReplaceUI(UITextType key, string text)
	{
		DebugLog.DebugWrite($"Replacing UI text \"{key}\" with \"{text}\"");
		var table = TextTranslation.Get().m_table;
		table.theUITable[(int)key] = text;
	}

	public static UITextType AddToUITable(string text)
	{
		var table = TextTranslation.Get().m_table;
		var key = table.theUITable.Keys.Max() + 1;
		table.theUITable[key] = text;
		DebugLog.DebugWrite($"Added text \"{text}\" to the UI table with index {key}");
		return (UITextType)key;
	}
}