using QSB.Utility;
using QSB.WorldSync;

namespace QSB.ZeroGCaveSync.WorldObjects
{
	internal class QSBSatelliteNode : WorldObject<SatelliteNode>
	{
		public void SetRepaired()
		{
			DebugLog.DebugWrite($"[SATELLITE NODE] {AttachedObject} Set repaired.");
			AttachedObject._damaged = false;
			var component = Locator.GetPlayerTransform().GetComponent<ReferenceFrameTracker>();
			if (component.GetReferenceFrame() == AttachedObject._rfVolume.GetReferenceFrame())
			{
				component.UntargetReferenceFrame();
			}

			if (AttachedObject._rfVolume != null)
			{
				AttachedObject._rfVolume.gameObject.SetActive(false);
			}

			if (AttachedObject._lanternLight != null)
			{
				AttachedObject._lanternLight.color = AttachedObject._lightRepairedColor;
			}

			if (AttachedObject._lanternEmissiveRenderer != null)
			{
				AttachedObject._lanternEmissiveRenderer.sharedMaterials.CopyTo(AttachedObject._lanternMaterials, 0);
				AttachedObject._lanternMaterials[AttachedObject._lanternMaterialIndex] = AttachedObject._lanternRepairedMaterial;
				AttachedObject._lanternEmissiveRenderer.sharedMaterials = AttachedObject._lanternMaterials;
			}

			AttachedObject.RaiseEvent(nameof(AttachedObject.OnRepaired), AttachedObject);
		}

		public void RepairTick(float repairFraction)
		{
			DebugLog.DebugWrite($"[SATELLITE NODE] {AttachedObject} repair tick {repairFraction}");
			AttachedObject._repairFraction = repairFraction;
			var damageEffect = AttachedObject._damageEffect;
			damageEffect.SetEffectBlend(1f - repairFraction);
		}
	}
}
