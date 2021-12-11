namespace QSB.ItemSync.WorldObjects.Items
{
	internal class QSBSlideReelItem : QSBOWItem<SlideReelItem>
	{
		public override void PlaySocketAnimation()
			=> AttachedObject.PlaySocketAnimation();

		public override void PlayUnsocketAnimation()
			=> AttachedObject.PlayUnsocketAnimation();

		public override void OnCompleteUnsocket()
			=> AttachedObject.OnCompleteUnsocket();
	}
}
