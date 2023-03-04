using QSB.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Player;

[UsedInUnityProject]
public class RemotePlayerRulesetDetector : Detector
{
	private PlanetoidRuleset _closestPlanetoidRuleset;
	private List<PlanetoidRuleset> _planetoidRulesets;

	public override void Awake()
	{
		base.Awake();
		_planetoidRulesets = new List<PlanetoidRuleset>(8);
	}

	public override void AddVolume(EffectVolume volume)
	{
		if ((volume as RulesetVolume) == null)
		{
			return;
		}

		base.AddVolume(volume);
		if (volume.GetType() == typeof(PlanetoidRuleset))
		{
			DebugLog.DebugWrite($"ADD TO {volume.name}");
			_planetoidRulesets.Add((PlanetoidRuleset)volume);
			UpdateClosestPlanetoidRuleset();
		}
	}

	public override void RemoveVolume(EffectVolume volume)
	{
		if ((volume as RulesetVolume) == null)
		{
			return;
		}

		base.RemoveVolume(volume);
		if (volume.GetType() == typeof(PlanetoidRuleset))
		{
			DebugLog.DebugWrite($"REMOVE FROM {volume.name}");
			_planetoidRulesets.Remove((PlanetoidRuleset)volume);
			UpdateClosestPlanetoidRuleset();
		}
	}

	public PlanetoidRuleset GetPlanetoidRuleset() => _closestPlanetoidRuleset;

	private void Update()
	{
		if (_planetoidRulesets.Count > 1)
		{
			UpdateClosestPlanetoidRuleset();
		}
	}

	private void UpdateClosestPlanetoidRuleset()
	{
		DebugLog.DebugWrite($"UpdateClosetPlanetoidRuleset - count:{_planetoidRulesets.Count}");
		_closestPlanetoidRuleset = null;
		var num = float.PositiveInfinity;
		for (var i = 0; i < _planetoidRulesets.Count; i++)
		{
			var num2 = Vector3.SqrMagnitude(_planetoidRulesets[i].transform.position - transform.position);
			if (num2 < num)
			{
				_closestPlanetoidRuleset = _planetoidRulesets[i];
				num = num2;
			}
		}
	}
}
