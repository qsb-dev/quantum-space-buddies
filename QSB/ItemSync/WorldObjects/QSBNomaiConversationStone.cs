namespace QSB.ItemSync.WorldObjects
{
	internal class QSBNomaiConversationStone : QSBOWItem<NomaiConversationStone>
	{
		public override void Init(NomaiConversationStone attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
