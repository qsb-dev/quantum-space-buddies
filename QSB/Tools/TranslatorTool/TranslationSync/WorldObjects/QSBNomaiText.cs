using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Tools.TranslatorTool.TranslationSync.WorldObjects;

public class QSBNomaiText : WorldObject<NomaiText>
{
	public void SetAsTranslated(int id) => AttachedObject.SetAsTranslated(id);

	public IEnumerable<int> GetTranslatedIds()
	{
		if (!AttachedObject._initialized)
		{
			// shouldn't happen, but does anyway sometimes. whatever lol
			return Array.Empty<int>();
		}

		return AttachedObject._dictNomaiTextData
			.Where(x => x.Value.IsTranslated)
			.Select(x => x.Key);
	}
}