using OWML.Utils;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;

namespace QSB.TranslationSync.WorldObjects
{
	internal class QSBVesselComputer : WorldObject<NomaiVesselComputer>
	{
		public override void Init(NomaiVesselComputer computer, int id)
		{
			ObjectId = id;
			AttachedObject = computer;
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
			var rings = AttachedObject.GetValue<NomaiVesselComputerRing[]>("_computerRings");
			return rings
				.Where(ring => AttachedObject.IsTranslated(ring.GetEntryID()))
				.Select(ring => ring.GetEntryID());
		}
	}
}