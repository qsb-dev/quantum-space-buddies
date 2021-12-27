using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Tools.TranslatorTool.TranslationSync.WorldObjects
{
	internal class QSBNomaiText : WorldObject<NomaiText>
	{
		public void SetAsTranslated(int id) => AttachedObject.SetAsTranslated(id);

		public IEnumerable<int> GetTranslatedIds() =>
			AttachedObject._dictNomaiTextData
				.Where(x => x.Value.IsTranslated)
				.Select(x => x.Key);
	}
}
