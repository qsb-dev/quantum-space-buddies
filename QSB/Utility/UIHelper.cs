using System.Linq;

namespace QSB.Utility
{
	internal static class UIHelper
	{
		public static int AddToUITable(string text)
		{
			var instance = UnityEngine.Object.FindObjectOfType<TextTranslation>().m_table;
			instance.Insert_UI(instance.theUITable.Keys.Max() + 1, text);
			return instance.theUITable.Keys.Max();
		}
	}
}
