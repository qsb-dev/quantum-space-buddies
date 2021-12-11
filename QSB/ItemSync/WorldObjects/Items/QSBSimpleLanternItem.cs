namespace QSB.ItemSync.WorldObjects.Items
{
	internal class QSBSimpleLanternItem : QSBOWItem<SimpleLanternItem>
	{
		public override void PlaySocketAnimation()
			=> AttachedObject.PlaySocketAnimation();

		public override void PlayUnsocketAnimation()
			=> AttachedObject.PlayUnsocketAnimation();

		public override void OnCompleteUnsocket()
			=> AttachedObject.OnCompleteUnsocket();
	}
}
