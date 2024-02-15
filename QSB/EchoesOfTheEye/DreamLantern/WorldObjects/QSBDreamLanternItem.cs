using QSB.ItemSync.WorldObjects.Items;
using UnityEngine;

namespace QSB.EchoesOfTheEye.DreamLantern.WorldObjects;

public class QSBDreamLanternItem : QSBItem<DreamLanternItem>
{
	public override void PickUpItem(Transform holdTransform)
	{
		base.PickUpItem(holdTransform);

		if (AttachedObject._lanternType != DreamLanternType.Nonfunctioning)
		{
			AttachedObject.gameObject.transform.localScale = Vector3.one * 2f;
		}

		AttachedObject.EnableInteraction(true);
	}

	public override void DropItem(Vector3 worldPosition, Vector3 worldNormal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
	{
		base.DropItem(worldPosition, worldNormal, parent, sector, customDropTarget);

		if (AttachedObject._lanternType != DreamLanternType.Nonfunctioning)
		{
			AttachedObject.gameObject.transform.localScale = Vector3.one;
		}

		// If in the DreamWorld, don't let other people pick up your lantern
		// Since this method is only called on the remote, this only makes other players unable to pick it up
		// If the lantern is lit, the user is in the DreamWorld
		AttachedObject.EnableInteraction(!AttachedObject.GetLanternController().IsLit());
	}
}
