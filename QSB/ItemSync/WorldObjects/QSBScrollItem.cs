namespace QSB.ItemSync.WorldObjects
{
	internal class QSBScrollItem : QSBOWItem<ScrollItem>
	{
		public override void Init(ScrollItem attachedObject, int id)
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

		public void HideNomaiText()
			=> AttachedObject.HideNomaiText();

		public void ShowNomaiText()
			=> AttachedObject.ShowNomaiText();
	}
}
