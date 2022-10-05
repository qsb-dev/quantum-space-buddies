using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace QSB.ItemSync.WorldObjects.Items;

public class QSBDreamLanternItem : QSBItem<DreamLanternItem> 
{
	private Material[] _materials;

	public override async UniTask Init(CancellationToken ct)
	{
		await base.Init(ct);

		// Some lanterns (ie, nonfunctioning) don't have a view model group
		if (AttachedObject._lanternType != DreamLanternType.Nonfunctioning)
		{
			_materials = AttachedObject._lanternController._viewModelGroup?.GetComponentsInChildren<MeshRenderer>(true)?.SelectMany(x => x.materials)?.ToArray();
		}
	}

	public override void PickUpItem(Transform holdTransform)
	{
		base.PickUpItem(holdTransform);

		// Fixes #502: Artifact is visible through the walls
		if (AttachedObject._lanternType != DreamLanternType.Nonfunctioning)
		{
			foreach (Material m in _materials)
			{
				if (m.renderQueue >= 2000)
				{
					m.renderQueue -= 2000;
				}
			}

			// The view model looks much smaller than the dropped item
			AttachedObject.gameObject.transform.localScale = Vector3.one * 2f;
		}
	}

	public override void DropItem(Vector3 worldPosition, Vector3 worldNormal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
	{
		base.DropItem(worldPosition, worldNormal, parent, sector, customDropTarget);

		if (AttachedObject._lanternType != DreamLanternType.Nonfunctioning)
		{
			foreach (Material m in _materials)
			{
				if (m.renderQueue < 2000)
				{
					m.renderQueue += 2000;
				}
			}

			AttachedObject.gameObject.transform.localScale = Vector3.one;
		}

		// If in the DreamWorld, don't let other people pick up your lantern
		// Since this method is only called on the remote, this only makes other players unable to pick it up
		// If the lantern is lit, the user is in the DreamWorld
		AttachedObject.EnableInteraction(!AttachedObject.GetLanternController().IsLit());
	}
}
