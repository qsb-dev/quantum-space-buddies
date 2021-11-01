namespace QSB.ItemSync.WorldObjects.Items
{
	class QSBSimpleLanternItem : QSBOWItem<SimpleLanternItem>
	{
		public override void Init(SimpleLanternItem attachedObject, int id)
		{
			ObjectId = id;
			AttachedObject = attachedObject;
			base.Init(attachedObject, id);
		}

		public override void PlaySocketAnimation()
			=> AttachedObject.PlaySocketAnimation();

		public override void PlayUnsocketAnimation()
			=> AttachedObject.PlayUnsocketAnimation();

		public override void OnCompleteUnsocket()
			=> AttachedObject.OnCompleteUnsocket();
	}
}
