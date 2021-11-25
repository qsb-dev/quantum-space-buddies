using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.Tools.TranslatorTool.TranslationSync.WorldObjects
{
	internal class QSBWallText : WorldObject<NomaiWallText>
	{
		public override void Init(NomaiWallText wallText, int id)
		{
			ObjectId = id;
			AttachedObject = wallText;
		}

		public void HandleSetAsTranslated(int id)
		{
			if (AttachedObject.IsTranslated(id))
			{
				return;
			}

			AttachedObject.SetAsTranslated(id);
		}

		public IEnumerable<int> GetTranslatedIds()
		{
			var dict = AttachedObject._idToNodeDict;
			return dict.Keys.Where(key => AttachedObject.IsTranslated(key));
		}
	}
}
