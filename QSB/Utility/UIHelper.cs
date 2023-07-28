using System.Linq;

namespace QSB.Utility;

public static class UIHelper
{
	public static void ReplaceUI(UITextType key, string text)
	{
		DebugLog.DebugWrite($"Replacing UI text \"{key}\" with \"{text}\"");
		var table = TextTranslation.Get().m_table;
		table.theUITable[(int)key] = text;
	}

	public static UITextType AddToUITable(string text)
	{
		var hasValue = UITableHasValue(text);
		if (hasValue != UITextType.None)
		{
			return hasValue;
		}

		var table = TextTranslation.Get().m_table;
		var key = table.theUITable.Keys.Max() + 1;
		table.theUITable[key] = text;
		DebugLog.DebugWrite($"Added text \"{text}\" to the UI table with index {key}");
		return (UITextType)key;
	}

	public static UITextType UITableHasValue(string text)
	{
		var table = TextTranslation.Get().m_table;
		return (UITextType)table.theUITable.FirstOrDefault(x => x.Value == text).Key;
	}
}