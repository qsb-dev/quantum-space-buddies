using OWML.Utils;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.Tools.TranslatorTool.TranslationSync.WorldObjects
{
	internal class QSBComputer : WorldObject<NomaiComputer>
	{
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
			var activeList = AttachedObject.GetValue<List<NomaiComputerRing>>("_activeRingList");
			foreach (var item in activeList)
			{
				if (AttachedObject.IsTranslated(item.GetEntryID()))
				{
					yield return item.GetEntryID();
				}
			}

			var inactiveList = AttachedObject.GetValue<List<NomaiComputerRing>>("_inactiveRingList");
			foreach (var item in inactiveList)
			{
				if (AttachedObject.IsTranslated(item.GetEntryID()))
				{
					yield return item.GetEntryID();
				}
			}
		}
	}
}
