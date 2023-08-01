using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using QSB.ZeroGCaveSync.Messages;

namespace QSB.ZeroGCaveSync.WorldObjects;

public class QSBSatelliteNode : WorldObject<SatelliteNode>
{
	public override void SendInitialState(uint to)
	{
		if (!AttachedObject._damaged)
		{
			this.SendMessage(new SatelliteNodeRepairedMessage { To = to });
		}

		this.SendMessage(new SatelliteNodeRepairTickMessage(AttachedObject._repairFraction) { To = to });
	}

	public void SetRepaired()
	{
		if (!AttachedObject._damaged)
		{
			return;
		}

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
		if (OWMath.ApproxEquals(AttachedObject._repairFraction, repairFraction))
		{
			return;
		}

		DebugLog.DebugWrite($"[SATELLITE NODE] {AttachedObject} repair tick {repairFraction}");
		AttachedObject._repairFraction = repairFraction;
		var damageEffect = AttachedObject._damageEffect;
		damageEffect.SetEffectBlend(1f - repairFraction);
	}
}