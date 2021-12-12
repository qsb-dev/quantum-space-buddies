namespace QSB.ItemSync.WorldObjects.Items
{
	internal class QSBSharedStone : QSBOWItem<SharedStone>
	{
		public override void PlaySocketAnimation()
			=> AttachedObject.PlaySocketAnimation();

		public override void PlayUnsocketAnimation()
			=> AttachedObject.PlayUnsocketAnimation();

		public override void OnCompleteUnsocket()
			=> AttachedObject.OnCompleteUnsocket();

		public NomaiRemoteCameraPlatform.ID GetRemoteCameraID()
			=> AttachedObject.GetRemoteCameraID();
	}
}
