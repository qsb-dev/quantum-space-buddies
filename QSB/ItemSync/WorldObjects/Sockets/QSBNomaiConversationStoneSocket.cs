namespace QSB.ItemSync.WorldObjects.Sockets
{
	internal class QSBNomaiConversationStoneSocket : QSBOWItemSocket<NomaiConversationStoneSocket>
	{
		public override void Init(NomaiConversationStoneSocket attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}
	}
}
