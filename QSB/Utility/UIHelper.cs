using QSB.Localisation;
using System.Globalization;
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

	public static CultureInfo GetCurrentCultureInfo() 
		=> QSBLocalization.Current.Language switch
		{
			/*
			 * Language tags from BCP-47 standard, implemented by windows
			 * https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
			 * I have no fucking idea if this will work on linux. ¯\_(ツ)_/¯
			 */

			TextTranslation.Language.ENGLISH => new CultureInfo("en"),
			TextTranslation.Language.SPANISH_LA => new CultureInfo("es-419"),
			TextTranslation.Language.GERMAN => new CultureInfo("de"),
			TextTranslation.Language.FRENCH => new CultureInfo("fr"),
			TextTranslation.Language.ITALIAN => new CultureInfo("it"),
			TextTranslation.Language.POLISH => new CultureInfo("pl"),
			TextTranslation.Language.PORTUGUESE_BR => new CultureInfo("pt-BR"),
			TextTranslation.Language.JAPANESE => new CultureInfo("ja"),
			TextTranslation.Language.RUSSIAN => new CultureInfo("ru"),
			TextTranslation.Language.CHINESE_SIMPLE => new CultureInfo("zh-Hans"),
			TextTranslation.Language.KOREAN => new CultureInfo("ko"),
			TextTranslation.Language.TURKISH => new CultureInfo("tr"),
			_ => new CultureInfo("en") // what
		};
}