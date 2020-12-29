using QSB.WorldSync;

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
	}
}
