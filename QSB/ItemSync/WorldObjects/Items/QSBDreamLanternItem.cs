using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items
{
	public class QSBDreamLanternItem : QSBItem<DreamLanternItem>
	{
		public override void DropItem(Vector3 position, Vector3 normal, Sector sector)
		{
			base.DropItem(position, normal, sector);
			AttachedObject.enabled = false;
			if (AttachedObject._lanternController != null)
			{
				AttachedObject._lanternController.SetDetectorScaleCompensation(AttachedObject._lanternController.transform.lossyScale);
				AttachedObject._lanternController.SetHeldByPlayer(false);
				AttachedObject._lanternController.enabled = AttachedObject._lanternController.IsLit();
			}
		}

		public override void SocketItem(Transform socketTransform, Sector sector)
		{
			base.SocketItem(socketTransform, sector);
			AttachedObject.enabled = false;
			if (AttachedObject._lanternController != null)
			{
				AttachedObject._lanternController.SetDetectorScaleCompensation(AttachedObject._lanternController.transform.lossyScale);
				AttachedObject._lanternController.SetSocketed(true);
				AttachedObject._lanternController.SetHeldByPlayer(false);
				AttachedObject._lanternController.enabled = AttachedObject._lanternController.IsLit();
			}
		}
	}
}