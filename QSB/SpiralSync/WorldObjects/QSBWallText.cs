using OWML.Utils;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.SpiralSync.WorldObjects
{
	internal class QSBWallText : WorldObject<NomaiWallText>
	{
		public override void Init(NomaiWallText quantumSocket, int id)
		{
			ObjectId = id;
			AttachedObject = quantumSocket;
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
			var dict = AttachedObject.GetValue<Dictionary<int, OWTreeNode<NomaiTextLine>>>("_idToNodeDict");
			foreach (var key in dict.Keys)
			{
				if (AttachedObject.IsTranslated(key))
				{
					yield return key;
				}
			}
		}
	}
}
