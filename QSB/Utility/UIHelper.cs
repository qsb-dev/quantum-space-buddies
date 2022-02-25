using System.Linq;

namespace QSB.Utility;

internal static class UIHelper
{
	public static void ReplaceUI(UITextType key, string text)
	{
		var table = TextTranslation.Get().m_table;
		table.theUITable[(int)key] = text;
	}

	public static UITextType AddToUITable(string text)
	{
		var table = TextTranslation.Get().m_table;
		var key = table.theUITable.Keys.Max() + 1;
		table.theUITable[key] = text;
		return (UITextType)key;
	}
}